import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { IParty } from "../types/Party"; // Import the IParty interface
import "./PartiesPage.css";

// --- Import Logos (keep this section as is) ---
import socialdemokratietLogo from "../images/PartyLogos/socialdemokratiet.webp";
import venstreLogo from "../images/PartyLogos/Venstre.png";
import moderaterneLogo from "../images/PartyLogos/Moderaterne.png";
import alternativetLogo from "../images/PartyLogos/alternativet.png";
import borgernesLogo from "../images/PartyLogos/borgernesParti.jpg";
import centrumLogo from "../images/PartyLogos/centrumDemokraterne.png";
import danmarksLogo from "../images/PartyLogos/danmarksDemokraterne.jpg";
import DFLogo from "../images/PartyLogos/DanskFolkeparti.png";
import enhedslistenLogo from "../images/PartyLogos/enhedslisten.jpg";
import inuitLogo from "../images/PartyLogos/InuitAtaqatigiit.png";
import javnaLogo from "../images/PartyLogos/Javnaðarflokkurin.png";
import konsvertiveLogo from "../images/PartyLogos/konservative.png";
import kristeligtLogo from "../images/PartyLogos/KristeligFolkeparti.png";
import LALogo from "../images/PartyLogos/LiberalAlliance.png";
import naleraq from "../images/PartyLogos/NaleraqLogo.svg";
import radikale from "../images/PartyLogos/radikaleVenstre.png";
import sambands from "../images/PartyLogos/sambandspartiet.png";
import SF from "../images/PartyLogos/SocialistiskeFolkeparti.png";
// --- End Logo Imports ---

// --- Logo Map (keep this section as is, ensuring keys match partyName from API) ---
const partyLogoMap: { [key: string]: string } = {
  Socialdemokratiet: socialdemokratietLogo,
  Venstre: venstreLogo,
  Moderaterne: moderaterneLogo,
  Alternativet: alternativetLogo,
  "Borgernes Parti": borgernesLogo, // Check if this name exists in your DB
  "Centrum-Demokraterne": centrumLogo, // Check if this name exists
  Danmarksdemokraterne: danmarksLogo,
  "Dansk Folkeparti": DFLogo,
  "Det Konservative Folkeparti": konsvertiveLogo,
  Enhedslisten: enhedslistenLogo,
  "Inuit Ataqatigiit": inuitLogo,
  Javnaðarflokkurin: javnaLogo,
  "Kristeligt Folkeparti": kristeligtLogo,
  "Liberal Alliance": LALogo,
  Naleraq: naleraq,
  "Radikale Venstre": radikale,
  Sambandsflokkurin: sambands,
  "Socialistisk Folkeparti": SF,
};
// --- End Logo Map ---

const PartiesPage: React.FC = () => {
  // *** Change state to hold IParty objects ***
  const [parties, setParties] = useState<IParty[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchParties = async () => {
      setLoading(true);
      setError(null);
      setParties([]);

      try {
        // *** Change API endpoint ***
        const apiUrl = `http://localhost:5218/api/Party/Parties`; // Use the new Party endpoint
        console.log(`Workspaceing parties from: ${apiUrl}`); // Debug log
        const response = await fetch(apiUrl);

        if (!response.ok) {
          let errorMsg = `HTTP error ${response.status}: ${response.statusText}`;
          try {
            const errorBody = await response.json();
            errorMsg = errorBody.message || errorBody.title || errorMsg;
          } catch {
            /* Ignore */
          }
          throw new Error(errorMsg);
        }

        // *** Change data type ***
        const data: IParty[] = await response.json();
        // Filter out any parties without a name, just in case
        setParties(data.filter((p) => p.partyName));
        console.log("Parties fetched:", data); // Debug log
      } catch (err: unknown) {
        console.error("Fetch error:", err);
        let message = "Kunne ikke hente partiliste"; // Default message
        if (err instanceof Error) {
          message = err.message;
        } else if (typeof err === "string") {
          message = err;
        }
        setError(message);
      } finally {
        setLoading(false);
      }
    };

    fetchParties();
  }, []); // Empty dependency array, fetch only once on mount

  // --- Loading and Error states remain the same ---
  if (loading) {
    return <div className="loading-message">Henter partier...</div>;
  }
  if (error) {
    return (
      <div className="error-message">
        Fejl: {error} <Link to="/">Tilbage</Link>
      </div>
    );
  }

  // --- Update Render Logic ---
  return (
    <div className="parties-page">
      <nav>
        <Link to="/">← Tilbage</Link>
      </nav>
      <h2>Partier</h2>

      {parties.length > 0 ? (
        <ul className="parties-grid-list">
          {/* *** Update map function *** */}
          {parties.map((party) => {
            // Ensure partyName is not null before using it
            const partyName = party.partyName || "Ukendt Parti";
            const logoSrc = partyLogoMap[partyName]; // Lookup logo using partyName

            // Handle cases where partyName might be null for the link
            if (!party.partyName) {
              console.warn(`Party with ID ${party.partyId} has a null name.`);
              return null; // Skip rendering this party if name is essential
            }

            return (
              // *** Use partyId for the key ***
              <li key={party.partyId}>
                {/* *** Link still uses partyName for the route param *** */}
                <Link to={`/party/${encodeURIComponent(party.partyName)}`} className="party-grid-link">
                  <img
                    src={logoSrc} // Use looked-up logo
                    alt={`${partyName} logo`}
                    className="party-grid-logo"
                    onError={(e) => {
                      (e.target as HTMLImageElement).style.display = "none";
                    }}
                  />
                  {/* *** Display partyName *** */}
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
