import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { IAktor } from '../types/Aktor';
import { IParty } from '../types/Party';
import PartyInfoCard from '../components/Party/PartyInfoCard';
import "./PartyPage.css"; // Ensure this CSS file is imported
import DefaultPic from "../images/defaultPic.jpg";
// --- Logo Map ---
import socialdemokratietLogo from '../images/PartyLogos/socialdemokratiet.webp';
import venstreLogo from '../images/PartyLogos/Venstre.png';
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
  "Venstre": venstreLogo,
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
  "Socialistisk Folkeparti": SF
};
// --- End Logo Map ---


const PartyPage: React.FC = () => {
  const { partyName } = useParams<{ partyName: string }>();

  // State variables
  const [partyDetails, setPartyDetails] = useState<IParty | null>(null);
  const [members, setMembers] = useState<IAktor[]>([]);
  const [loadingParty, setLoadingParty] = useState<boolean>(true);
  const [loadingMembers, setLoadingMembers] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // --- State for role holders ---
  const [chairman, setChairman] = useState<IAktor | null>(null);
  const [viceChairman, setViceChairman] = useState<IAktor | null>(null);
  const [secretary, setSecretary] = useState<IAktor | null>(null);
  const [spokesperson, setSpokesperson] = useState<IAktor | null>(null);
  const [groupLeader, setGroupLeader] = useState<IAktor | null>(null);
  const [deputyGroupLeader, setDeputyGroupLeader] = useState<IAktor | null>(null);


  const displayPartyName = partyName ? decodeURIComponent(partyName) : 'Ukendt Parti';
  const defaultPoliticianImageUrl = DefaultPic;

  // --- Helper function to fetch Aktor details by ID ---
  const fetchAktorById = async (id: number | null): Promise<IAktor | null> => {
    if (!id) return null;
    try {
      const response = await fetch(`http://localhost:5218/api/Aktor/${id}`);
      if (!response.ok) {
        console.error(`Failed to fetch Aktor with ID ${id}: ${response.status}`);
        return null;
      }
      return await response.json() as IAktor;
    } catch (err) {
      console.error(`Error fetching Aktor with ID ${id}:`, err);
      return null;
    }
  };

  // Effect to fetch party details, role holders, and then members
  useEffect(() => {
    const fetchPartyAndMembers = async () => {
      if (!partyName) {
        setError("Partinavn mangler i URL.");
        setLoadingParty(false);
        return;
      }

      // Reset states
      setLoadingParty(true);
      setLoadingMembers(false);
      setError(null);
      setPartyDetails(null);
      setMembers([]);
      setChairman(null);
      setViceChairman(null);
      setSecretary(null);
      setSpokesperson(null);
      setGroupLeader(null);
      setDeputyGroupLeader(null);


      // --- Step 1: Fetch Party Details ---
      try {
        const partyApiUrl = `http://localhost:5218/api/Party/Party/${encodeURIComponent(partyName)}`;
        const partyResponse = await fetch(partyApiUrl);

        if (!partyResponse.ok) {
          if (partyResponse.status === 404) {
             throw new Error(`Partiet "${displayPartyName}" blev ikke fundet.`);
          } else {
             let errorMsg = `HTTP error ${partyResponse.status}: ${partyResponse.statusText}`;
             try { const errorBody = await partyResponse.json(); errorMsg = errorBody.message || errorBody.title || errorMsg; } catch { /* Ignore */ }
             throw new Error(errorMsg);
          }
        }

        const fetchedPartyData: IParty = await partyResponse.json();
        setPartyDetails(fetchedPartyData);

        // --- Step 2: Fetch Role Holder Details (concurrently) ---
        if (fetchedPartyData) {
          const rolePromises = [
            fetchAktorById(fetchedPartyData.chairmanId),
            fetchAktorById(fetchedPartyData.viceChairmanId),
            fetchAktorById(fetchedPartyData.secretaryId),
            fetchAktorById(fetchedPartyData.spokesmanId)
            // Add fetches for group leader etc. if IDs become available in IParty
            // e.g., fetchAktorById(fetchedPartyData.groupLeaderId),
          ];

          const [
            fetchedChairman,
            fetchedViceChairman,
            fetchedSecretary,
            fetchedSpokesperson
            // ...destructure other results here: fetchedGroupLeader, fetchedDeputyGroupLeader
          ] = await Promise.all(rolePromises);

          setChairman(fetchedChairman);
          setViceChairman(fetchedViceChairman);
          setSecretary(fetchedSecretary);
          setSpokesperson(fetchedSpokesperson);
          // setGroupLeader(fetchedGroupLeader);
          // setDeputyGroupLeader(fetchedDeputyGroupLeader);
        }


        // --- Step 3: Fetch Member Details ---
        if (fetchedPartyData && fetchedPartyData.memberIds && fetchedPartyData.memberIds.length > 0) {
          setLoadingMembers(true);
          const memberFetchPromises = fetchedPartyData.memberIds.map(id => fetchAktorById(id)); // Reuse helper
          const memberResults = await Promise.all(memberFetchPromises);
          const validMembers = memberResults.filter(member => member !== null) as IAktor[];
          setMembers(validMembers);
        }

      } catch (err: unknown) {
        console.error("Fetch error:", err);
        let message = `Kunne ikke hente data for partiet ${displayPartyName}`;
        if (err instanceof Error) message = err.message;
        else if (typeof err === 'string') message = err;
        setError(message);
      } finally {
        setLoadingParty(false);
        setLoadingMembers(false);
      }
    };

    fetchPartyAndMembers();

  }, [partyName, displayPartyName]);

  // --- Render Logic ---
  const isLoading = loadingParty; // Base loading on party details primarily

  if (isLoading) return <div className="loading-message">Henter data for {displayPartyName}...</div>;
  if (error) return <div className="error-message">Fejl: {error} <Link to="/parties">Tilbage til partioversigt</Link></div>;
  if (!partyDetails) return <div className="info-message">Kunne ikke finde partiinformation.</div>;

  // Helper to get logo URL
  const getLogoUrl = (name: string | null): string | undefined => {
      return name ? partyLogoMap[name] : undefined;
  }

  return (
    <div className="party-page">
      <nav className="party-page-nav">
        <Link to="/parties">← Tilbage til partioversigt</Link>
      </nav>

       {/* --- NEW: Main Content Area (Flex Container) --- */}
      <div className="party-main-content">

          {/* --- Left Column: Details Sections --- */}
          <div className="party-details-column">
              {/* Optional: Add overall party title here if needed */}
              {/* <h1>{partyDetails.partyName || displayPartyName}</h1> */}

              {partyDetails.partyProgram && (
                 <section className="party-details-section">
                     <h3>Partiprogram</h3>
                     <p className="party-details-content">{partyDetails.partyProgram}</p>
                 </section>
              )}
               {partyDetails.politics && (
                 <section className="party-details-section">
                     <h3>Politik</h3>
                     <p className="party-details-content">{partyDetails.politics}</p>
                 </section>
              )}
               {partyDetails.history && (
                 <section className="party-details-section">
                     <h3>Historie</h3>
                     <p className="party-details-content">{partyDetails.history}</p>
                 </section>
              )}
          </div>
          {/* --- End Left Column --- */}

          {/* --- Right Column: Info Card --- */}
          <div className="party-infobox-column">
              <PartyInfoCard
                partyName={partyDetails.partyName || displayPartyName}
                slogan={undefined} // Add slogan from partyDetails if/when available
                logoUrl={getLogoUrl(partyDetails.partyName)}
                defaultLogo={DefaultPic} // Use the imported default politician pic as fallback
                chairmanName={chairman?.navn}
                viceChairmanName={viceChairman?.navn}
                secretaryName={secretary?.navn}
                politicalSpokespersonName={spokesperson?.navn}
                groupLeaderName={groupLeader?.navn} // Pass / Pass fetched or null
              />
          </div>
          {/* --- End Right Column --- */}

      </div>
      {/* --- End Main Content Area --- */}


      {/* --- Member List (Below the columns) --- */}
      <section className="party-members-section">
         <h3>Medlemmer</h3>
         {loadingMembers ? (
            <div className="loading-message">Henter medlemmer...</div>
         ) : members.length > 0 ? (
           <ul className="party-member-list">
             {members.map((politician) => (
               <li key={politician.id}>
                 <Link to={`/politician/${politician.id}`} className="party-member-link">
                   <img
                     src={politician.pictureMiRes || defaultPoliticianImageUrl}
                     alt={`Portræt af ${politician.navn}`}
                     className="party-member-photo"
                     onError={(e) => {
                       const imgElement = e.target as HTMLImageElement;
                       if (imgElement.src !== defaultPoliticianImageUrl) {
                         imgElement.src = defaultPoliticianImageUrl;
                       } else {
                         imgElement.style.display = 'none';
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
             Ingen medlemmer fundet for partiet "{displayPartyName}".
             {(partyDetails.memberIds === null || partyDetails.memberIds.length === 0) && " (Ingen medlems-ID'er registreret)"}
           </p>
         )}
      </section>
    </div>
  );
};

export default PartyPage;
