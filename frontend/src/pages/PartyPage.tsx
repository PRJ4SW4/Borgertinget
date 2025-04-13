// src/pages/PartyPage.tsx
import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { IAktor } from '../types/Aktor';
import "./PartyPage.css"
import DefaultPic from "../images/defaultPic.jpg";

// ... (useState, useEffect, fetch logic remain the same) ...

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
        setError("Party name is missing from URL.");
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
           if (response.status === 404) { /* Handled by length check later */ }
           else { let errorMsg = `HTTP error ${response.status}: ${response.statusText}`; try { const errorBody = await response.json(); errorMsg = errorBody.message || errorBody.title || errorMsg; } catch(e) {} throw new Error(errorMsg); }
        }
        const data: IAktor[] = await response.json();
        setPoliticians(data);
      } catch (err: any) {
        console.error("Fetch error:", err);
        setError(err.message || `Failed to fetch data for party ${displayPartyName}`);
      } finally { setLoading(false); }
    };
    fetchPartyPoliticians();
  }, [partyName]);

  if (loading) return <div className="loading-message">Loader politikere for {displayPartyName}...</div>;
  if (error) return <div className="error-message">Error: {error} <Link to="/">Go back home</Link></div>;

  return (
    <div className="party-page">
      <nav>
          <Link to="/">← Tilbage til listen</Link>
      </nav>
      <h1>{displayPartyName}</h1>
      <h3>Medlemmer</h3>

      {politicians.length > 0 ? (
        <ul className="party-member-list">
          {politicians.map((politician) => (
            <li key={politician.id}>
              <Link to={`/politician/${politician.id}`} className="party-member-link">
                {/* --- Updated Image Logic --- */}
                <img
                  // Use actual image source if available, otherwise use default
                  src={politician.pictureMiRes || defaultImageUrl}
                  alt={`Portrait of ${politician.navn}`}
                  className="party-member-photo" // Or styles.partyMemberPhoto if using CSS Modules
                  onError={(e) => {
                    const imgElement = e.target as HTMLImageElement;

                    // Check if the src is already the default to prevent infinite loops
                    if (imgElement.src !== window.location.origin + defaultImageUrl) {
                      console.warn(`Failed to load image: ${politician.pictureMiRes}. Using default.`);
                      // Set to default image on error
                      imgElement.src = defaultImageUrl;
                    } else {
                      // If even the default image failed, log error and optionally hide
                      console.error(`Failed to load default image: ${defaultImageUrl}`);
                      imgElement.style.display = 'none'; // Example: hide the broken image space
                    }
                  }}
                />
                {/* --- End Updated Image Logic --- */}

                <span className="party-member-name">{politician.navn}</span>
              </Link>
            </li>
          ))}
        </ul>
      ) : (
        <p className="info-message">
        Det var ikke muligt at finde nogen politikere "{displayPartyName}".
        </p>
      )}
    </div>
  );
};

export default PartyPage;