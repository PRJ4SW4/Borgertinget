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
  const displayPartyName = partyName ? decodeURIComponent(partyName) : 'Unknown Party';
  const defaultImageUrl = DefaultPic;

  useEffect(() => {
    const fetchPartyPoliticians = async () => {
      if (!partyName) {
        setError("Partinavn mangler i URL.");
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
          if (response.status === 404) {
          } else {
             let errorMsg = `HTTP error ${response.status}: ${response.statusText}`;
             try {
                const errorBody = await response.json();
                errorMsg = errorBody.message || errorBody.title || errorMsg;
             } catch { // deliberately empty 
             }
             throw new Error(errorMsg); // Throw the consolidated error message
          }
        }

        
        const data: IAktor[] = response.ok ? await response.json() : []; // Safely parse or default to empty array
        setPoliticians(data);

      } catch (err: unknown) {
        console.error("Fetch error:", err);
        // Type checking before accessing properties
        let message = `Kunne ikke hente data for partiet ${displayPartyName}`; // Default message
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
    
  }, [partyName, displayPartyName]);

  if (loading) return <div className="loading-message">Loader politikere for {displayPartyName}...</div>;
 
  if (error) return <div className="error-message">Error: {error} <Link to="/">Tilbage til forsiden</Link></div>;

  return (
    <div className="party-page">
      <nav>
         <Link to="/parties">← Tilbage til partioversigt</Link>
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
                  alt={`Portræt af ${politician.navn}`}
                  className="party-member-photo"
                  onError={(e) => {
                    const imgElement = e.target as HTMLImageElement;
                    if (imgElement.src !== defaultImageUrl) {
                        console.warn(`Kunne ikke loade billede: ${politician.pictureMiRes}. Bruger standardbillede.`);
                        imgElement.src = defaultImageUrl;
                    } else {
                        console.error(`Kunne ikke loade standardbillede: ${defaultImageUrl}`);
                        imgElement.style.display = 'none'; // Hide broken image
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
          {error ? `Fejl under hentning af data.` : `Ingen politikere fundet for partiet "${displayPartyName}".`}
        </p>
      )}
    </div>
  );
};

export default PartyPage;