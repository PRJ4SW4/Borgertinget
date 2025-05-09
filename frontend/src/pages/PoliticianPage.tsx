import React, { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import { IAktor } from "../types/Aktor";
import "./PoliticianPage.css";
// Consider adding a default image import if you need one for onError
// import DefaultPic from "../images/defaultPic.jpg";
import DefaultPic from "../images/defaultPic.jpg"; // Import your default picture
//Af Jakob, dette er til subscribe knappen
import SubscribeButton from "../components/FeedComponents/SubscribeButton";
import { getSubscriptions } from "../services/tweetService";

const PoliticianPage: React.FC = () => {
  const { id } = useParams<{ id: string }>(); // Step 1: id (e.g., "3") is extracted from the URL.
  const [politician, setPolitician] = useState<IAktor | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  //Af Jakob, dette er til subscribe knappen
  const [isSubscribed, setIsSubscribed] = useState<boolean>(false);
  const [twitterId, setTwitterId] = useState<number | null>(null);

  const defaultImageUrl = DefaultPic; // Use the imported default image

  // --- Helper: Calculate Age ---
  const calculateAge = (birthDateString?: string | null): number | null => {
    if (!birthDateString) return null;
    // Attempt to handle different date formats if necessary, assuming YYYY-MM-DD or similar
    try {
      // More robust parsing might be needed depending on actual 'born' format
      const birthDate = new Date(birthDateString);
      if (isNaN(birthDate.getTime())) return null; // Invalid date parsed

      const today = new Date();
      let age = today.getFullYear() - birthDate.getFullYear();
      const m = today.getMonth() - birthDate.getMonth();
      if (m < 0 || (m === 0 && today.getDate() < birthDate.getDate())) {
        age--;
      }
      return age;
    } catch (e) {
      console.error("Error parsing birth date:", e);
      return null;
    }
  };

  // --- Helper: Format Birth Date ---
  const formatBirthDate = (birthDateString?: string | null): string => {
    if (!birthDateString) return "Ikke angivet";
    try {
      const date = new Date(birthDateString);
      if (isNaN(date.getTime())) return "Ugyldig dato";
      // Format as DD-MM-YYYY (adjust locale and options as needed)
      return date.toLocaleDateString("da-DK", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      });
    } catch (e) {
      console.error("Error formatting birth date:", e);
      return "Formateringsfejl";
    }
  };

  useEffect(() => {
    const fetchPolitician = async () => {
      if (!id || isNaN(Number(id))) {
        setError("Ugyldigt politiker-ID i URL.");
        setLoading(false);
        return;
      }
      setLoading(true);
      setError(null);
      setPolitician(null);

      try {
        // Step 2: The id from the URL is used to fetch the Aktor data.
        const apiUrl = `http://localhost:5218/api/Aktor/${id}`;
        const response = await fetch(apiUrl);

        if (!response.ok) {
          if (response.status === 404) {
            throw new Error(`Politiker med ID ${id} blev ikke fundet.`);
          } else {
            let errorMsg = `HTTP error ${response.status}: ${response.statusText}`;
            try {
              const errorBody = await response.json();
              errorMsg = errorBody.message || errorBody.title || errorMsg;
            } catch {
              /* Ignore */
            }
            throw new Error(errorMsg);
          }
        }
        const data: IAktor = await response.json();
        // The politician state is set here. politician.id will be the id from the fetched Aktor data (e.g., 3).
        setPolitician(data);
      } catch (err: unknown) {
        console.error("Fetch error:", err);
        let message = `Kunne ikke hente data for politiker ${id}`;
        if (err instanceof Error) message = err.message;
        else if (typeof err === "string") message = err;
        setError(message);
      } finally {
        setLoading(false);
      }
    };

    fetchPolitician();
  }, [id]); // Dependency array is correct, only depends on id

  // Af Jakob, dette er til subscribe knappen
  useEffect(() => {
    const checkSubscriptionStatus = async () => {
      if (politician && politician.id) {
        // politician.id here is, for example, 3.
        try {
          // Hent brugerens eksisterende abonnementer
          const subscriptions = await getSubscriptions();

          // Step 3: politician.id (e.g., 3) is used as aktorId in this API call.
          // If the database has AktorId=206 for this politician in PoliticianTwitterId table,
          // but politician.id is 3, it indicates a mismatch between the Aktor loaded by URL
          // and the expected AktorId in the PoliticianTwitterId table.
          const lookupResponse = await fetch(`http://localhost:5218/api/subscription/lookup/politicianTwitterId?aktorId=${politician.id}`, {
            headers: {
              Authorization: `Bearer ${localStorage.getItem("jwt")}`,
            },
          });

          if (lookupResponse.ok) {
            const lookupData = await lookupResponse.json();
            setTwitterId(lookupData.politicianTwitterId);

            // Tjek om politikeren allerede følges
            setIsSubscribed(subscriptions.some((sub) => sub.id === lookupData.politicianTwitterId));
          } else {
            console.error("Kunne ikke finde Twitter ID for denne politiker");
          }
        } catch (error) {
          console.error("Fejl ved tjek af abonnementsstatus:", error);
        }
      }
    };

    checkSubscriptionStatus();
  }, [politician]);

  // --- Loading & Error States ---
  if (loading) return <div className="loading-message">Henter politiker detaljer...</div>;
  if (error)
    return (
      <div className="error-message">
        Fejl: {error} <Link to="/">Tilbage til forsiden</Link>
      </div>
    );
  if (!politician)
    return (
      <div className="info-message">
        Politikerdata er ikke tilgængelig. <Link to="/">Tilbage til forsiden</Link>
      </div>
    );

  // --- Calculate Age ---
  const age = calculateAge(politician.born);
  const formattedBornDate = formatBirthDate(politician.born);

  // --- Image Error Handler ---
  const handleImageError = (e: React.SyntheticEvent<HTMLImageElement, Event>) => {
    const imgElement = e.target as HTMLImageElement;
    if (imgElement.src !== defaultImageUrl) {
      console.warn(`Kunne ikke loade billede: ${politician.pictureMiRes}. Bruger standardbillede.`);
      imgElement.src = defaultImageUrl; // Use the imported default image
    } else {
      console.error(`Kunne ikke loade standardbillede: ${defaultImageUrl}`);
      imgElement.style.display = "none"; // Hide if default also fails
    }
  };

  // --- Render Component ---
  return (
    <div className="politician-page">
      {/* Navigation remains outside the main layout */}
      <nav className="politician-page-nav">
        {politician.party ? (
          <Link to={`/party/${encodeURIComponent(politician.party)}`}>← Tilbage til {politician.party}</Link>
        ) : (
          <Link to="/parties">← Tilbage til partioversigt</Link>
        )}
      </nav>
      {/* Gray Information Box */}
      <div className="info-box">
        {politician.pictureMiRes ? ( // Use ternary for cleaner conditional rendering
          <img
            src={politician.pictureMiRes}
            alt={`Portræt af ${politician.navn || "Politiker"}`} // Danish alt text
            className="info-box-photo"
            onError={(e: React.SyntheticEvent<HTMLImageElement, Event>) => {
              const imgElement = e.target as HTMLImageElement;
              console.error(`Kunne ikke loade billede: ${politician.pictureMiRes}`); // Danish
              imgElement.style.display = "none"; // Simple hide on error
            }}
          />
        ) : (
          <div className="info-box-photo-placeholder">Intet billede</div> // Danish
        )}
        <h4>Navn</h4>
        {/* Use politician's name or fallback */}
        <p>{politician.fornavn && politician.efternavn ? `${politician.fornavn} ${politician.efternavn}` : politician.navn || "Ukendt"}</p>

        <h4>Parti</h4>
        <p>
          {politician.party ? (
            <Link to={`/party/${encodeURIComponent(politician.party)}`}>{politician.party}</Link>
          ) : (
            politician.partyShortname || "Partiløs/Ukendt" // Show shortname or indicate independent/unknown
          )}
        </p>
        <h4>Email</h4>
        <p>{politician.email ? <a href={`mailto:${politician.email}`}>{politician.email}</a> : "Ikke tilgængelig"}</p>

        {/* Conditional rendering for lists inside the info-box */}
        {politician.educations && politician.educations.length > 0 && (
          <>
            <h4>Uddannelse</h4>
            <ul>
              {politician.educations.map((edu, index) => (
                <li key={`edu-${index}`}>{edu}</li>
              ))}
            </ul>
          </>
        )}

        {politician.constituencies && politician.constituencies.length > 0 && (
          <>
            <h4>Embede / Valgkreds</h4> {/* More specific title */}
            <ul>
              {politician.constituencies.map((con, index) => (
                <li key={`con-${index}`}>{con}</li>
              ))}
            </ul>
          </>
        )}
      </div>{" "}
      {/* END: Info Box */}
      {/* Other Details Outside the Box */}
      <article className="politician-details">
        <section className="detail-section">
          <h3>Grundlæggende Information</h3> {/* Danish */}
          <p>
            <strong>Født:</strong> {politician.born || "Ikke tilgængelig"}
          </p>{" "}
          {/* Danish */}
          <p>
            <strong>Titel:</strong> {politician.functionFormattedTitle || "Ikke tilgængelig"}
          </p>{" "}
          {/* Danish */}
        </section>

        {/* Display other lists NOT in the info-box */}
        {politician.publicationTitles && politician.publicationTitles.length > 0 && (
          <>
            <section className="detail-section">
              <h3>Baggrund</h3>
              <p className="detail-content-placeholder">
                Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam id commodo dolor. Class aptent taciti sociosqu ad litora torquent per
                conubia nostra, per inceptos himenaeos. Suspendisse fermentum nisi a venenatis hendrerit. Curabitur mauris nunc, sodales ac lacinia
                eget, consectetur et arcu.
              </p>
              {/* Add more placeholder text or logic to extract from biografi */}
            </section>
          </>
        )}

        <section className="detail-section">
          <h3>Begyndende politisk karriere</h3>
          <p className="detail-content-placeholder">
            Integer feugiat tempus venenatis. Sed tempor massa tortor, fringilla suscipit ante eleifend ac. Proin sit amet vestibulum nulla. Maecenas
            et turpis sit amet lectus commodo facilisis ac sed leo. Donec a lacinia libero, id placerat urna.
            {/* Add more placeholder text or logic to extract from biografi */}
          </p>
        </section>

        {/* --- Existing Sections Moved Here --- */}
        {politician.ministertitel && (
          <section className="detail-section">
            <h3>Nuværende minister post</h3>
            <p>{politician.ministertitel}</p>
          </section>
        )}
        {politician.ministers && politician.ministers.length > 0 && (
          <section className="detail-section">
            <h3>Ministerposter</h3>
            <ul>
              {politician.ministers.map((min, index) => (
                <li key={`min-${index}`}>{min}</li>
              ))}
            </ul>
          </section>
        )}
        {politician.spokesmen && politician.spokesmen.length > 0 && (
          <section className="detail-section">
            <h3>Ordførerskaber</h3>
            <ul>
              {politician.spokesmen.map((spk, index) => (
                <li key={`spk-${index}`}>{spk}</li>
              ))}
            </ul>
          </section>
        )}
        {politician.parliamentaryPositionsOfTrust && politician.parliamentaryPositionsOfTrust.length > 0 && (
          <section className="detail-section">
            <h3>Tillidshverv (Parlamentarisk)</h3>
            <ul>
              {politician.parliamentaryPositionsOfTrust.map((ptrust, index) => (
                <li key={`ptrust-${index}`}>{ptrust}</li>
              ))}
            </ul>
          </section>
        )}
        {politician.positionsOfTrust && politician.positionsOfTrust.length > 0 && (
          <section className="detail-section">
            <h3>Tillidshverv (ikke-parliamentarisk)</h3>
            {/* Check if it's a simple string or needs mapping */}
            {typeof politician.positionsOfTrust === "string" ? (
              <p>{politician.positionsOfTrust}</p> // Render directly if string
            ) : (
              <ul>
                {politician.positionsOfTrust.map((trust, index) => (
                  <li key={`trust-${index}`}>{trust}</li>
                ))}
              </ul>
            )}
          </section>
        )}
        {politician.nominations && politician.nominations.length > 0 && (
          <section className="detail-section">
            <h3>Kandidaturer</h3>
            <ul>
              {politician.nominations.map((nom, index) => (
                <li key={`nom-${index}`}>{nom}</li>
              ))}
            </ul>
          </section>
        )}
        {politician.occupations && politician.occupations.length > 0 && (
          <section className="detail-section">
            <h3>Beskæftigelse</h3>
            <ul>
              {politician.occupations.map((occ, index) => (
                <li key={`occ-${index}`}>{occ}</li>
              ))}
            </ul>
          </section>
        )}
        {politician.publicationTitles && politician.publicationTitles.length > 0 && (
          <section className="detail-section">
            <h3>Forfatterskab</h3>
            <ul>
              {politician.publicationTitles.map((title, index) => (
                <li key={`pub-${index}`}>{title}</li>
              ))}
            </ul>
          </section>
        )}
        {/* Add other sections as needed */}

        {/* --- End Left Column --- */}

        {/* --- Right Column: Info Box --- */}
        <div className="politician-infobox-column">
          <div className="info-box">
            {/* Image */}
            <div className="info-box-image-container">
              <img
                src={politician.pictureMiRes || defaultImageUrl}
                alt={`Portræt af ${politician.navn || "Politiker"}`}
                className="info-box-photo"
                onError={handleImageError}
              />
            </div>
            {/*Subscribe*/}
            {twitterId !== null && (
              <div className="subscription-container">
                <SubscribeButton
                  politicianTwitterId={twitterId}
                  initialIsSubscribed={isSubscribed}
                  onSubscriptionChange={(newStatus) => setIsSubscribed(newStatus)}
                />
              </div>
            )}
            {/* Details */}
            <h3>{politician.navn || "Ukendt Navn"}</h3>
            <p className="info-box-role">
              {politician.functionFormattedTitle || "Medlem af Folketinget"}
              {politician.party && `, `}
              {politician.party && <Link to={`/party/${encodeURIComponent(politician.party)}`}>{politician.party}</Link>}
              {politician.ministertitel && ` (${politician.ministertitel})`}
            </p>
            <hr className="info-box-divider" />
            {/* Use helper function for info items */}
            {renderInfoItem("Adresse", "[Ikke tilgængelig]")} {/* Placeholder */}
            {renderInfoItem("Alder + føds", `${formattedBornDate}${age !== null ? ` - ${age} år` : ""}`)}
            {renderInfoItem("Tlf", "[Ikke tilgængelig]")} {/* Placeholder */}
            {renderInfoItem("Email", politician.email ? <a href={`mailto:${politician.email}`}>{politician.email}</a> : "Ikke tilgængelig")}
            {renderInfoItem("Hjemmeside", "[Ikke tilgængelig]")} {/* Placeholder - need to parse from bio or add field */}
            {/* Add Education and Constituency lists if desired in info box */}
            {politician.educations && politician.educations.length > 0 && (
              <>
                <hr className="info-box-divider" />
                <h4>Uddannelse</h4>
                <ul className="info-box-list">
                  {politician.educations.map((edu, index) => (
                    <li key={`edu-${index}`}>{edu}</li>
                  ))}
                </ul>
              </>
            )}
            {/* {politician.constituencies && politician.constituencies.length > 0 && ( ... )} */}
          </div>
        </div>
        {/* --- End Right Column --- */}
      </article>
      {/* --- End Main Content Area --- */}
    </div>
  );
};

// Helper function to render info box items consistently
const renderInfoItem = (label: string, value: React.ReactNode) => (
  <div className="info-box-item">
    <span className="info-box-label">{label}:</span>
    <span className="info-box-value">{value}</span>
  </div>
);

export default PoliticianPage;
