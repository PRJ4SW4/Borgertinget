// src/pages/PoliticianPage.tsx
import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { IAktor } from '../types/Aktor';
import "./PoliticianPage.css";
// Consider adding a default image import if you need one for onError
// import DefaultPic from "../images/defaultPic.jpg";

//Af Jakob, dette er til subscribe knappen
import SubscribeButton from '../components/FeedComponents/SubscribeButton';
import { getSubscriptions } from '../services/tweetService';

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
      // Validate ID early
      if (!id || isNaN(Number(id))) { // Check if id exists and is a number
        setError("Ugyldigt politiker-ID i URL."); // Danish error message
        setLoading(false);
        return;
      }

      setLoading(true);
      setError(null);
      setPolitician(null); // Clear previous data

      try {
        const apiUrl = `http://localhost:5218/api/Aktor/${id}`;
        const response = await fetch(apiUrl);

        if (!response.ok) {
          if (response.status === 404) {
            // Throw specific error for 404
            throw new Error(`Politiker med ID ${id} blev ikke fundet.`); // Danish
          } else {
            // Handle other HTTP errors
            let errorMsg = `HTTP error ${response.status}: ${response.statusText}`;
            // Try to get a more specific error from the response body
            try {
              const errorBody = await response.json();
              errorMsg = errorBody.message || errorBody.title || errorMsg;
            } catch  { // <-- Fix 1 & 2: Use _e and add comment
              /* Intentional: Ignore error parsing the error body, already have status text */
            }
            throw new Error(errorMsg); // Throw the consolidated error message
          }
        }

        // Only parse JSON if response is OK
        const data: IAktor = await response.json();
        setPolitician(data);

      } catch (err: unknown) { // <-- Fix 3: Use unknown instead of any
        console.error("Fetch error:", err);
        // Type checking before accessing properties
        let message = `Kunne ikke hente data for politiker ${id}`; // Default message (Danish)
        if (err instanceof Error) {
            message = err.message; // Use message property if it's an Error
        } else if (typeof err === 'string') {
            message = err; // Use the error directly if it's a string
        }
        // Set the extracted or default error message
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
      if (politician && politician.id) { // KORREKT: aktørId → id
        try {
          // Hent brugerens eksisterende abonnementer
          const subscriptions = await getSubscriptions();
          
          // Find Twitter ID via lookup-API'et (behold aktorId som parameter i URL)
          const token = localStorage.getItem('jwt');
          // Clean the token by removing any quotes that might be wrapping it
          const cleanToken = token ? token.replace(/^["'](.*)["']$/, '$1') : '';

          const lookupResponse = await fetch(`http://localhost:5218/api/subscription/lookup/politicianTwitterId?aktorId=${politician.id}`, { 
            headers: {
              'Authorization': `Bearer ${cleanToken}`
            }
          });

          if (lookupResponse.ok) {
            const lookupData = await lookupResponse.json();
            setTwitterId(lookupData.politicianTwitterId);
            
            // Tjek om politikeren allerede følges
            setIsSubscribed(subscriptions.some(sub => sub.id === lookupData.politicianTwitterId));
          } else {
            console.error('Kunne ikke finde Twitter ID for denne politiker');
          }
        } catch (error) {
          console.error('Fejl ved tjek af abonnementsstatus:', error);
        }
      }
    };
    
    checkSubscriptionStatus();
  }, [politician]);


  // --- Render logic (update link texts to Danish) ---
  if (loading) return <div className="loading-message">Henter politiker detaljer...</div>;
  if (error) return <div className="error-message">Fejl: {error} <Link to="/">Tilbage til forsiden</Link></div>;
  if (!politician) return <div className="info-message">Politikerdata er ikke tilgængelig. <Link to="/">Tilbage til forsiden</Link></div>;

  // --- Politician details rendering logic (update link texts and potentially onError for image) ---
  return (
    <div className="politician-page">
      <nav>
          {/* Determine back link based on party info or default */}
          {politician.party ? (
             <Link to={`/party/${encodeURIComponent(politician.party)}`}>← Tilbage til {politician.party}</Link>
          ) : (
             <Link to="/partier">← Tilbage til partioversigt</Link> // Fallback to general parties list
          )}
      </nav>

      {/* Gray Information Box */}
      <div className="info-box">
        {politician.pictureMiRes ? ( // Use ternary for cleaner conditional rendering
          <img
            src={politician.pictureMiRes}
            alt={`Portræt af ${politician.navn || 'Politiker'}`} // Danish alt text
            className="info-box-photo"
            onError={(e: React.SyntheticEvent<HTMLImageElement, Event>) => {
                const imgElement = e.target as HTMLImageElement;
                console.error(`Kunne ikke loade billede: ${politician.pictureMiRes}`); // Danish
                imgElement.style.display = 'none'; // Simple hide on error
            }}
          />
        ) : (
           <div className="info-box-photo-placeholder">Intet billede</div> // Danish
        )}
        <h4>Navn</h4>
        {/* Use politician's name or fallback */}
        <p>{politician.fornavn && politician.efternavn ? `${politician.fornavn} ${politician.efternavn}` : (politician.navn || 'Ukendt')}</p>



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
            <Link to={`/party/${encodeURIComponent(politician.party)}`}>
              {politician.party}
            </Link>
          ) : (
            politician.partyShortname || 'Partiløs/Ukendt' // Show shortname or indicate independent/unknown
          )}
        </p>
        <h4>Email</h4>
        <p>{politician.email ? <a href={`mailto:${politician.email}`}>{politician.email}</a> : 'Ikke tilgængelig'}</p>

        {/* Conditional rendering for lists inside the info-box */}
        {politician.educations && politician.educations.length > 0 && (
           <>
             <h4>Uddannelse</h4>
             <ul>
               {politician.educations.map((edu, index) => <li key={`edu-${index}`}>{edu}</li>)}
             </ul>
           </>
         )}

        {politician.constituencies && politician.constituencies.length > 0 && (
           <>
             <h4>Embede / Valgkreds</h4> {/* More specific title */}
             <ul>
               {politician.constituencies.map((con, index) => <li key={`con-${index}`}>{con}</li>)}
             </ul>
           </>
         )}
      </div> {/* END: Info Box */}


      {/* Other Details Outside the Box */}
      <article className="politician-details">
        
        <section className="detail-section">
            <h3>Grundlæggende Information</h3> {/* Danish */}
            <p><strong>Født:</strong> {politician.born || 'Ikke tilgængelig'}</p> {/* Danish */}
            <p><strong>Titel:</strong> {politician.functionFormattedTitle || 'Ikke tilgængelig'}</p> {/* Danish */}
        </section>

        {/* Display other lists NOT in the info-box */}
        {politician.publicationTitles && politician.publicationTitles.length > 0 && (
          <section className="detail-section">
            <h3>Publikationer (Titler)</h3> {/* Danish */}
            <ul>
              {politician.publicationTitles.map((title, index) => <li key={`pub-${index}`}>{title}</li>)}
            </ul>
          </section>
        )}
        {politician.nominations && politician.nominations.length > 0 && (
           <section className="detail-section">
               <h3>Nomineringer</h3> {/* Danish */}
               <ul>
                   {politician.nominations.map((nom, index) => <li key={`nom-${index}`}>{nom}</li>)}
               </ul>
           </section>
         )}
        {politician.occupations && politician.occupations.length > 0 && (
          <section className="detail-section">
            <h3>Beskæftigelse</h3> {/* Danish */}
            <ul>
              {politician.occupations.map((occ, index) => <li key={`occ-${index}`}>{occ}</li>)}
            </ul>
          </section>
        )}
         {/* Add Minister Posts / Spokesperson Roles / Positions of Trust if needed */}
         {politician.ministers && politician.ministers.length > 0 && (
           <section className="detail-section">
               <h3>Ministerposter</h3>
               <ul>{politician.ministers.map((min, index) => <li key={`min-${index}`}>{min}</li>)}</ul>
           </section>
         )}
          {politician.spokesmen && politician.spokesmen.length > 0 && (
           <section className="detail-section">
               <h3>Ordførerskaber</h3>
               <ul>{politician.spokesmen.map((spk, index) => <li key={`spk-${index}`}>{spk}</li>)}</ul>
           </section>
         )}
          {politician.positionsOfTrust && politician.positionsOfTrust.length > 0 && (
           <section className="detail-section">
               <h3>Tillidshverv (Eksternt)</h3>
               <ul>{politician.positionsOfTrust.map((trust, index) => <li key={`trust-${index}`}>{trust}</li>)}</ul>
           </section>
         )}
          {politician.parliamentaryPositionsOfTrust && politician.parliamentaryPositionsOfTrust.length > 0 && (
           <section className="detail-section">
               <h3>Tillidshverv (Parlamentarisk)</h3>
               <ul>{politician.parliamentaryPositionsOfTrust.map((ptrust, index) => <li key={`ptrust-${index}`}>{ptrust}</li>)}</ul>
           </section>
         )}

      </article> {/* End Other Details */}
    </div>
  );
};

export default PoliticianPage;