// src/pages/PartiesPage.tsx
import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom'; // Import Link for navigation
import "./PartiesPage.css"

import socialdemokratietLogo from '../images/PartyLogos/socialdemokratiet.webp'; // Example
import venstreLogo from '../images/PartyLogos/Venstre.png';             // Example
import moderaterneLogo from '../images/PartyLogos/Moderaterne.png';  
import alternativetLogo from '../images/PartyLogos/alternativet.png';
import borgernesLogo from '../images/PartyLogos/borgernesParti.jpg';
import centrumLogo from '../images/PartyLogos/centrumDemokraterne.png';
import danmarksLogo from '../images/PartyLogos/danmarksDemokraterne.jpg';
import DFLogo from '../images/PartyLogos/DanskFolkeparti.png';
import enhedslistenLogo from '../images/PartyLogos/enhedslisten.jpg';
import inuitLogo from '../images/PartyLogos/InuitAtaqatigiit.png';
import javnaLogo from '../images/PartyLogos/Javnaðarflokkurin.png';
import konsvertiveLogo from '../images/PartyLogos/konservative.png';
import kristeligtLogo from '../images/PartyLogos/KristeligFolkeparti.png';
import LALogo from '../images/PartyLogos/LiberalAlliance.png';
import naleraq from '../images/PartyLogos/NaleraqLogo.svg';
import radikale from '../images/PartyLogos/radikaleVenstre.png';
import sambands from '../images/PartyLogos/sambandspartiet.png';
import SF from '../images/PartyLogos/SocialistiskeFolkeparti.png';
const partyLogoMap: { [key: string]: string } = {
  "Socialdemokratiet": socialdemokratietLogo,
  "Venstre": venstreLogo, // Use the full name if that's what the API returns
  "Moderaterne": moderaterneLogo,
  "Alternativet": alternativetLogo,
  "Borgernes Parti": borgernesLogo,
  "Centrum-Demokraterne": centrumLogo,
  "Danmarksdemokraterne": danmarksLogo,
  "Dansk Folkeparti": DFLogo,
  "Det Konservative Folkeparti": konsvertiveLogo,
  "Enhedslisten": enhedslistenLogo,
  "Inuit Ataqatigiit": inuitLogo,
  "Javnaðarflokkurin": javnaLogo,
  "Kristeligt Folkeparti": kristeligtLogo,
  "Liberal Alliance": LALogo,
  "Naleraq": naleraq,
  "Radikale Venstre": radikale,
  "Sambandsflokkurin": sambands,
  "Socialistisk Folkeparti": SF,
  // ... add mappings for other parties ...
  // Parties not listed here will use the defaultLogo
};

const PartiesPage: React.FC = () => {
  const [parties, setParties] = useState<string[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);



  useEffect(() => {
    // Fetch the list of unique party names when the component mounts
    const fetchParties = async () => {
      setLoading(true);
      setError(null);
      setParties([]); // Clear previous results

      try {
        // Use the new backend endpoint api/Aktor/GetParties
        const apiUrl = `http://localhost:5218/api/Aktor/GetParties`;
        const response = await fetch(apiUrl);

        if (!response.ok) {
          let errorMsg = `HTTP error ${response.status}: ${response.statusText}`;
          // Try to parse a more specific error message from the response body
          try {
             const errorBody = await response.json();
             // Use error message from body if available, otherwise keep the status text
             errorMsg = errorBody.message || errorBody.title || errorMsg;
          } catch { // <-- Fix 1 & 2: Use _e and add comment
            /* Intentional: Ignore error parsing the error body, already have status text */
          }
          throw new Error(errorMsg); // Throw the consolidated error message
        }

        const data: string[] = await response.json();
        setParties(data);

      } catch (err: unknown) { // <-- Fix 3: Use unknown instead of any
        console.error("Fetch error:", err);
        // Type checking before accessing properties
        let message = 'Failed to fetch list of parties'; // Default message
        if (err instanceof Error) {
            message = err.message; // Use message property if it's an Error
        } else if (typeof err === 'string') {
            message = err; // Use the error directly if it's a string
        }
        // You could add more checks here for other error types if needed
        setError(message);
      } finally {
        setLoading(false);
      }
    };

    fetchParties();
  }, []); // Empty dependency array means this runs once on mount

  // --- Render loading state ---
  if (loading) {
    return <div className="loading-message">Loading partier...</div>;
  }

  // --- Render error state ---
  if (error) {
    return <div className="error-message">Error: {error} <Link to="/">Tilbage</Link></div>;
  }

  // --- Render parties list ---
  return (
    <div className="parties-page">
      <nav>
          <Link to="/">← Tilbage</Link>
      </nav>
      <h2>Partier</h2>

      {parties.length > 0 ? (
        // --- Use a different class for the grid container ---
        <ul className="parties-grid-list">
          {parties.map((partyName) => {
            // --- Step 3: Get the correct logo source ---
            const logoSrc = partyLogoMap[partyName];

            return (
              <li key={partyName}>
                 {/* --- Link wraps the grid item content --- */}
                <Link to={`/party/${encodeURIComponent(partyName)}`} className="party-grid-link">
                   {/* --- Display Logo --- */}
                  <img
                    src={logoSrc}
                    alt={`${partyName} logo`}
                    className="party-grid-logo"
                    // Basic onError to hide if even default fails
                    onError={(e) => { (e.target as HTMLImageElement).style.display = 'none';}}
                  />
                  {/* --- Display Party Name --- */}
                  <span className="party-grid-name">{partyName}</span>
                </Link>
              </li>
            );
          })}
        </ul>
      ) : (
        <p className="info-message">Ingen partier fundet</p>
      )}
    </div>
  );
};

export default PartiesPage;