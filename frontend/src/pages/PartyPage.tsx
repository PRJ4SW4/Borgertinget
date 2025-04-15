// src/pages/PartyPage.tsx
import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { IAktor } from '../types/Aktor';
import "./PartyPage.css"
import DefaultPic from "../images/defaultPic.jpg";

const PartyPage: React.FC = () => {
  const { partyName } = useParams<{ partyName: string }>();
  const [politicians, setPoliticians] = useState<IAktor[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  // Calculate displayPartyName outside useEffect, derived from partyName
  const displayPartyName = partyName ? decodeURIComponent(partyName) : 'Unknown Party';
  const defaultImageUrl = DefaultPic;

  useEffect(() => {
    const fetchPartyPoliticians = async () => {
      // Early exit if partyName is somehow missing (though useParams usually ensures it's string | undefined)
      if (!partyName) {
        setError("Partinavn mangler i URL."); // Changed error message to Danish
        setLoading(false);
        return;
      }

      setLoading(true);
      setError(null);
      setPoliticians([]);

      try {
        const apiUrl = `http://localhost:5218/api/Aktor/GetParty/${encodeURIComponent(partyName)}`;
        const response = await fetch(apiUrl);

        if (!response.ok) {
          // Handle 404 specifically maybe (or just let it result in empty data)
          if (response.status === 404) {
             // It's okay for a party to have no members, or the endpoint might 404.
             // We'll handle this by checking politicians.length later.
             // No explicit error needed here unless the API guarantees non-404 for valid parties.
          } else {
             // Handle other HTTP errors
             let errorMsg = `HTTP error ${response.status}: ${response.statusText}`;
             // Try to get a more specific error from the response body
             try {
                const errorBody = await response.json();
                errorMsg = errorBody.message || errorBody.title || errorMsg;
             } catch { // <-- Fix 1 & 2: Use _e and add comment
                /* Intentional: Ignore error parsing the error body, already have status text */
             }
             throw new Error(errorMsg); // Throw the consolidated error message
          }
        }

        // Only parse JSON if response is OK or a non-error status we want to handle (like 404 potentially returning empty array)
        // If you expect 404 to *not* return JSON, adjust logic here. Assuming 404 might return `[]` or throw above.
        const data: IAktor[] = response.ok ? await response.json() : []; // Safely parse or default to empty array
        setPoliticians(data);

      } catch (err: unknown) { // <-- Fix 3: Use unknown instead of any
        console.error("Fetch error:", err);
        // Type checking before accessing properties
        let message = `Kunne ikke hente data for partiet ${displayPartyName}`; // Default message (Danish)
        if (err instanceof Error) {
            message = err.message; // Use message property if it's an Error
        } else if (typeof err === 'string') {
            message = err; // Use the error directly if it's a string
        }
        setError(message);
      } finally {
        setLoading(false);
      }
    };

    fetchPartyPoliticians();
    // --- Fix 4: Add displayPartyName to dependency array ---
  }, [partyName, displayPartyName]); // <--- Added displayPartyName here

  // --- Render logic remains the same ---
  if (loading) return <div className="loading-message">Loader politikere for {displayPartyName}...</div>;
  // Update error message link text to Danish
  if (error) return <div className="error-message">Error: {error} <Link to="/">Tilbage til forsiden</Link></div>;

  return (
    <div className="party-page">
      <nav>
         {/* Update link text to Danish */}
         <Link to="/partier">← Tilbage til partioversigt</Link>
      </nav>
      <h1>{displayPartyName}</h1>
      <h3>Medlemmer</h3>

      {politicians.length > 0 ? (
        <ul className="party-member-list">
          {politicians.map((politician) => (
            <li key={politician.id}>
              <Link to={`/politician/${politician.id}`} className="party-member-link">
                <img
                  src={politician.pictureMiRes || defaultImageUrl}
                  alt={`Portræt af ${politician.navn}`} // Updated alt text to Danish
                  className="party-member-photo"
                  onError={(e) => {
                    const imgElement = e.target as HTMLImageElement;
                    // Prevent infinite loop if default image itself fails
                    if (imgElement.src !== defaultImageUrl) { // Simpler check if defaultImageUrl is absolute/relative path
                        console.warn(`Kunne ikke loade billede: ${politician.pictureMiRes}. Bruger standardbillede.`); // Danish
                        imgElement.src = defaultImageUrl;
                    } else {
                        console.error(`Kunne ikke loade standardbillede: ${defaultImageUrl}`); // Danish
                        imgElement.style.display = 'none'; // Hide broken image space
                    }
                  }}
                />
                <span className="party-member-name">{politician.navn}</span>
              </Link>
            </li>
          ))}
        </ul>
      ) : (
        <p className="info-message">
           {/* Updated message to Danish */}
          {error ? `Fejl under hentning af data.` : `Ingen politikere fundet for partiet "${displayPartyName}".`}
        </p>
      )}
    </div>
  );
};

export default PartyPage;