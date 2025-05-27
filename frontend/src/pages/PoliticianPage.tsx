import React, { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import { IAktor } from "../types/Aktor";
import "./PoliticianPage.css";
//Af Jakob, dette er til subscribe knappen
import SubscribeButton from "../components/FeedComponents/SubscribeButton";
import { getSubscriptions } from "../services/tweetService";

const PoliticianPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [politician, setPolitician] = useState<IAktor | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  //Af Jakob, dette er til subscribe knappen
  const [isSubscribed, setIsSubscribed] = useState<boolean>(false);
  const [twitterId, setTwitterId] = useState<number | null>(null);

  useEffect(() => {
    const fetchPolitician = async () => {
      if (!id || isNaN(Number(id))) {
        // Check if id exists and is a number
        setError("Ugyldigt politiker-ID i URL.");
        setLoading(false);
        return;
      }
      // Clear previous data
      setLoading(true);
      setError(null);
      setPolitician(null);

      try {
        const apiUrl = `http://localhost:5218/api/Aktor/${id}`;
        const response = await fetch(apiUrl, {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            authorization: `Bearer ${localStorage.getItem("jwt")}`,
          },
        });

        if (!response.ok) {
          if (response.status === 404) {
            throw new Error(`Politiker med ID ${id} blev ikke fundet.`);
          } else {
            let errorMsg = `HTTP error ${response.status}: ${response.statusText}`;
            try {
              const errorBody = await response.json();
              errorMsg = errorBody.message || errorBody.title || errorMsg;
            } catch {
              //Deliberately empty
            }
            throw new Error(errorMsg);
          }
        }
        const data: IAktor = await response.json();
        setPolitician(data);
      } catch (err: unknown) {
        console.error("Fetch error:", err);
        let message = `Kunne ikke hente data for politiker ${id}`;
        if (err instanceof Error) {
          message = err.message;
        } else if (typeof err === "string") {
          message = err; // Use the error directly if it's a string
        }
        // Set the extracted or default error message
        setError(message);
      } finally {
        setLoading(false);
      }
    };

    fetchPolitician();
  }, [id]);

  // Af Jakob, dette er til subscribe knappen
  useEffect(() => {
    const checkSubscriptionStatus = async () => {
      if (politician && politician.id) {
        // KORREKT: aktørId → id
        try {
          // Hent brugerens eksisterende abonnementer
          const subscriptions = await getSubscriptions();

          // Find Twitter ID via lookup-API'et (behold aktorId som parameter i URL)
          const lookupResponse = await fetch(`http://localhost:5218/api/subscription/lookup/politicianTwitterId?aktorId=${politician.id}`, {
            // KORREKT: aktørId → id
            headers: {
              Authorization: `Bearer ${localStorage.getItem("jwt")}`,
            },
          });

          if (lookupResponse.ok) {
            const lookupData = await lookupResponse.json();
            setTwitterId(lookupData.id);

            // Tjek om politikeren allerede følges
            setIsSubscribed(subscriptions.some((sub) => sub.id === lookupData.id));
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

  return (
    <div className="politician-page">
      <nav>
        { }
        {politician.party ? (
          <Link to={`/party/${encodeURIComponent(politician.party)}`}>← Tilbage til {politician.party}</Link>
        ) : (
          <Link to="/parties">← Tilbage til partioversigt</Link> // Fallback to general parties list
        )}
      </nav>
      {/* Gray Information Box */}
      <div className="info-box">
        {politician.pictureMiRes ? (
          <img
            src={politician.pictureMiRes}
            alt={`Portræt af ${politician.navn || "Politiker"}`}
            className="info-box-photo"
            onError={(e: React.SyntheticEvent<HTMLImageElement, Event>) => {
              const imgElement = e.target as HTMLImageElement;
              console.error(`Kunne ikke loade billede: ${politician.pictureMiRes}`);
              imgElement.style.display = "none";
            }}
          />
        ) : (
          <div className="info-box-photo-placeholder">Intet billede</div> // Danish
        )}
        <h4>Navn</h4>
        <p>{politician.fornavn && politician.efternavn ? `${politician.fornavn} ${politician.efternavn}` : politician.navn || "Ukendt"}</p>

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

        <h4>Parti</h4>
        <p>
          {politician.party ? (
            <Link to={`/party/${encodeURIComponent(politician.party)}`}>{politician.party}</Link>
          ) : (
            politician.partyShortname || "Partiløs/Ukendt"
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
            <h4>Embede / Valgkreds</h4>
            <ul>
              {politician.constituencies.map((con, index) => (
                <li key={`con-${index}`}>{con}</li>
              ))}
            </ul>
          </>
        )}
      </div>
      {/* END: Info Box */}

      {/* Other Details Outside the Box */}
      <article className="politician-details">
        <section className="detail-section">
          <h3>Grundlæggende Information</h3>
          <p>
            <strong>Født:</strong> {politician.born || "Ikke tilgængelig"}
          </p>
          <p>
            <strong>Titel:</strong> {politician.functionFormattedTitle || "Ikke tilgængelig"}
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
            <h3>Forfatterskab</h3>
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
        {politician.ministertitel && (
          <>
            <h3>Nuværende minister post</h3>
            <p>{politician.ministertitel}</p>
          </>
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
      </article>
    </div>
  );
};

export default PoliticianPage;
