import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { IParty } from "../types/Party";
import "./PartiesPage.css";

// --- Import Logos ---
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

const partyLogoMap: { [key: string]: string } = {
  Socialdemokratiet: socialdemokratietLogo,
  Venstre: venstreLogo,
  Moderaterne: moderaterneLogo,
  Alternativet: alternativetLogo,
  "Borgernes Parti": borgernesLogo,
  "Centrum-Demokraterne": centrumLogo,
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
  const [parties, setParties] = useState<IParty[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchParties = async () => {
      setLoading(true);
      setError(null);
      setParties([]);

      try {
        const apiUrl = `http://localhost:5218/api/Party/Parties`;
        console.log(`Workspaceing parties from: ${apiUrl}`);
        const response = await fetch(apiUrl, {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            authorization: `Bearer ${localStorage.getItem("jwt")}`,
          },
        });

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

        const data: IParty[] = await response.json();
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
  }, []);

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

  return (
    <div className="parties-page">
      <nav>
        <Link to="/">← Tilbage</Link>
      </nav>
      <h2>Partier</h2>

      {parties.length > 0 ? (
        <ul className="parties-grid-list">
          {parties.map((party) => {
            // Ensure partyName is not null before using it
            const partyName = party.partyName || "Ukendt Parti";
            const logoSrc = partyLogoMap[partyName]; // Lookup logo using partyName

            // Handle cases where partyName might be null for the link
            if (!party.partyName) {
              console.warn(`Party with ID ${party.partyId} has a null name.`);
              return null;
            }

            return (
              <li key={party.partyId}>
                <Link to={`/party/${encodeURIComponent(party.partyName)}`} className="party-grid-link">
                  <img
                    src={logoSrc}
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
