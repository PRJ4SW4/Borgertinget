// src/pages/PartiesPage.tsx
import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom'; // Import Link for navigation

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
           try { const errorBody = await response.json(); errorMsg = errorBody.message || errorBody.title || errorMsg; } catch(e) {}
           throw new Error(errorMsg);
        }

        const data: string[] = await response.json();
        setParties(data);

      } catch (err: any) {
        console.error("Fetch error:", err);
        setError(err.message || `Failed to fetch list of parties`);
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
          <Link to="/">← Tilbage</Link> {/* Navigation back */}
      </nav>
      <h2>Partier</h2>

      {parties.length > 0 ? (
        <ul className="party-list"> {/* Use a class for styling */}
          {parties.map((partyName) => (
            <li key={partyName}>
              {/* Link each name to the individual party page */}
              {/* Encode the party name for the URL path */}
              <Link to={`/party/${encodeURIComponent(partyName)}`}>
                {partyName}
              </Link>
            </li>
          ))}
        </ul>
      ) : (
        <p className="info-message">Ingen partier fundet</p>
      )}
    </div>
  );
};

export default PartiesPage;