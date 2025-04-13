// src/pages/PoliticianPage.tsx
import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { IAktor } from '../types/Aktor';
import "./PoliticianPage.css";

const PoliticianPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [politician, setPolitician] = useState<IAktor | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    // Fetch logic remains the same...
    const fetchPolitician = async () => {
        if (!id) {
          setError("Politician ID is missing from URL.");
          setLoading(false);
          return;
        }
        setLoading(true);
        setError(null);
        setPolitician(null);
        try {
          const apiUrl = `http://localhost:5218/api/Aktor/${id}`;
          const response = await fetch(apiUrl);
          if (!response.ok) {
             if (response.status === 404) { throw new Error(`Politician with ID ${id} not found.`); }
             else { let errorMsg = `HTTP error ${response.status}: ${response.statusText}`; try { const errorBody = await response.json(); errorMsg = errorBody.message || errorBody.title || errorMsg; } catch(e) {} throw new Error(errorMsg); }
          }
          const data: IAktor = await response.json();
          setPolitician(data);
        } catch (err: any) {
          console.error("Fetch error:", err);
          setError(err.message || `Failed to fetch data for politician ${id}`);
        } finally { setLoading(false); }
      };
      fetchPolitician();
  }, [id]);

  if (loading) return <div className="loading-message">Loading politician details...</div>;
  if (error) return <div className="error-message">Error: {error} <Link to="/">Go back home</Link></div>;
  if (!politician) return <div className="info-message">Politician data not available. <Link to="/">Go back home</Link></div>;

  // --- Render politician details with the new info-box ---
  return (
    <div className="politician-page">
      <nav>
          <Link to="/">← Back to List</Link>
      </nav>

      {/* NEW: The Gray Information Box */}
      <div className="info-box">
          {politician.pictureMiRes && (
            <img
              src={politician.pictureMiRes}
              alt={`Portrait of ${politician.navn || 'Politician'}`}
              className="info-box-photo" // New class for the photo inside the box
              onError={(e: React.SyntheticEvent<HTMLImageElement, Event>) => {
                 console.error(`Failed to load image: ${politician.pictureMiRes}`);
                 (e.target as HTMLImageElement).style.display = 'none';
              }}
            />
          )}
          <h4>Navn</h4>
          <p>{politician.navn || "who knows?"}</p>
          <h4>Parti</h4>
          <p>
            {politician.party || politician.partyShortname ? ( // Check if either party name exists
                <Link
                to={`/party/${encodeURIComponent(
                    politician.party || politician.partyShortname || '' // Use full name, fallback to shortname
                )}`}
                >
                {politician.party || politician.partyShortname} {/* Display full name or shortname */}
                </Link>
            ) : (
                'N/A' // Display N/A if neither exists
            )}
           </p>
           <h4>Email</h4>
          <p> {politician.email || 'N/A'}</p>
          
          {politician.educations && politician.educations.length > 0 && (
            <> {/* Use Fragment to group without adding extra div */}
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
                <h4>Embede</h4>
                <ul>
                    {politician.constituencies.map((con, index) => (
                        <li key={`con-${index}`}>{con}</li>
                    ))}
                </ul>
            </>
          )}
      </div>
      {/* END: Info Box */}


      {/* --- Other Details Outside the Box --- */}
      <article className="politician-details">
         {/* Move details NOT required in the box here */}
         {/* Or keep all details in one article and style the info-box within it */}

         <h2>{politician.navn || 'N/A'}</h2> {/* Name might be here or above the box */}

         <section className="detail-section">
            <h3>Basic Information (if not in box)</h3>
            <p><strong>Born:</strong> {politician.born || 'N/A'}</p>
            <p><strong>Title:</strong> {politician.functionFormattedTitle || 'N/A'}</p>
         </section>

         {/* Display other lists NOT in the info-box */}
         {politician.publicationTitles && politician.publicationTitles.length > 0 && (
           <section className="detail-section">
             <h3>Publications (Titles)</h3>
             <ul>
               {politician.publicationTitles.map((title, index) => (
                 <li key={`pub-${index}`}>{title}</li>
               ))}
             </ul>
           </section>
         )}
         {politician.nominations && politician.nominations.length > 0 && (
            <section className="detail-section">
                <h3>Nominations</h3>
                <ul>
                    {politician.nominations.map((nom, index) => (
                        <li key={`nom-${index}`}>{nom}</li>
                    ))}
                </ul>
            </section>
         )}
         {politician.occupations && politician.occupations.length > 0 && (
            <section className="detail-section">
                <h3>Occupations</h3>
                <ul>
                    {politician.occupations.map((occ, index) => (
                        <li key={`occ-${index}`}>{occ}</li>
                    ))}
                </ul>
            </section>
         )}

         {/* Add other relevant fields or lists from IAktor as needed */}

      </article>
      {/* --- End Other Details --- */}
    </div>
  );
};

export default PoliticianPage;