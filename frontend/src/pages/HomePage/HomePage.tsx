import React, { useState, ChangeEvent, KeyboardEvent } from 'react';
// Import Link from react-router-dom if you haven't already
import { Link } from 'react-router-dom'; // ADD THIS LINE
import './HomePage.css';
import { SearchDocument } from '../../types/searchResult';

const HomePage: React.FC = () => {
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [searchResults, setSearchResults] = useState<SearchDocument[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const handleInputChange = (event: ChangeEvent<HTMLInputElement>) => {
    setSearchQuery(event.target.value);
    if (searchResults.length > 0) {
        setSearchResults([]);
    }
    if (error) {
        setError(null);
    }
  };

  const executeSearch = async () => {
    if (!searchQuery.trim()) {
      setSearchResults([]);
      setError(null);
      return;
    }
    setIsLoading(true);
    setError(null);
    setSearchResults([]);

    try {
      const encodedQuery = encodeURIComponent(searchQuery);
      const response = await fetch(`/api/Search?query=${encodedQuery}`);
      if (!response.ok) {
        throw new Error(`Search failed with status: ${response.status}`);
      }
      const data: SearchDocument[] = await response.json();
      setSearchResults(data);
    } catch (err) {
      console.error("Search error:", err);
      setError(err instanceof Error ? err.message : 'An unknown error occurred during search.');
      setSearchResults([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'Enter') {
      executeSearch();
    }
  };

  // --- Helper function to generate links for search results ---
  const getResultLink = (result: SearchDocument): string => {
    // The ID from OpenSearch is like "aktor-123" or "flashcard-456"
    const parts = result.id.split('-');
    const actualId = parts.length > 1 ? parts.slice(1).join('-') : result.id; // handles IDs that might have hyphens

    switch (result.dataType.toLowerCase()) {
      case 'aktor':
        return `/politician/${actualId}`; // Assuming this is your route for a single politician
      case 'flashcard':
        if (result.collectionId) {
          return `/flashcards/${result.collectionId}`; // Link to the collection
        }
        return `/flashcards`; // Fallback or a general flashcards page
      case 'party':
        return `/party/${result.partyName}`
      case 'page':
        return `/learning/${actualId}`
      // Add more cases for other dataTypes if needed
      default:
        return '#'; // Default fallback link
    }
    
  };
  const getDataTypeDisplayName = (dataType: string): string => {
    switch (dataType.toLowerCase()) {
      case 'aktor':
        return 'Politiker';
      case 'flashcard':
        return 'Flashcard';
      case 'party':
        return 'Parti';
      case 'page':
        return 'Læringsside';
      // more cases (learning env)
      default:
        return dataType;
    }
  };
  // --- End helper function ---

  return (
    <div className="homepage">
      <section className="hero-section">
        <div className="hero-content">
          <img src="/assets/logo-large-white.png" alt="Borgertinget Stort Logo" className="hero-logo" />
          <h1 className="hero-title">Borgertinget</h1>
          <p className="hero-subtitle">Din stemme, din viden, din fremtid</p>

          <div className="hero-search-container">
            <input
              type="search"
              placeholder="Søg på tværs af Borgertinget"
              className="hero-search-input"
              value={searchQuery}
              onChange={handleInputChange}
              onKeyDown={handleKeyDown}
            />
          </div>

          <div className="search-results-container">
            {isLoading && <p className="search-loading">Søger...</p>}
            {error && <p className="search-error">Fejl: {error}</p>}
            {!isLoading && !error && searchQuery && searchResults.length === 0 && (
              <p className="search-no-results">Ingen resultater fundet for "{searchQuery}".</p>
            )}
            {searchResults.length > 0 && (
              <ul className="search-results-list">
                {searchResults.map((result) => (
                  <li key={result.id} className="search-result-item">
                    <Link to={getResultLink(result)} className="search-result-link">
                      <span className={`result-type-badge type-${getDataTypeDisplayName(result.dataType).toLowerCase()}`}>
                        {getDataTypeDisplayName(result.dataType)}
                      </span>
                      <span className="result-title">
                        { result.aktorName || result.frontText || result.title || result.pageTitle || result.backText || 'Ukendt Titel'}
                      </span>
                      {/* Optionally display a snippet of content */}
                      {/* {result.content && <p className="result-content-snippet">{result.content.substring(0, 100)}...</p>} */}
                    </Link>
                  </li>
                ))}
              </ul>
            )}
          </div>

          <p className="hero-prompt">
            Ikke sikker på, hvor du skal starte?{' '}
            <a href="#features" className="hero-prompt-link">
              Udforsk Danmark's politiske læringsplatform nedenfor
            </a>
          </p>
          <a href="#features" className="hero-scroll-down" aria-label="Scroll down">
            ↓
          </a>
        </div>
      </section>

      <div id="features" className="features-container">
        {/* Feature Sections ... */}
        <section className="feature-section">
          <div className="feature-text">
            <h2>Politik 101</h2>
            <p>En introduktion til politik i Danmark</p>
            <div className="feature-buttons">
                <a href="/learning/1"><button className="feature-button">Læs Politik 101</button></a>
                <a href="/flashcards/1"><button className="feature-button">Øv med Flashcards</button></a>
            </div>
          </div>
          <div className="feature-image">
            <img src="/assets/images/verdipolitik.png" alt="Værdipolitisk akse" />
          </div>
        </section>

        <section className="feature-section alt-layout">
          <div className="feature-text">
            <h2>Partierne & Politikerne</h2>
            <p>En oversigt over partierne og deres politikere</p>
            <div className="feature-buttons">
                <a href="/parties"><button className="feature-button">Partier</button></a>
                <a href="/politicians"><button className="feature-button">Politikere</button></a>
            </div>
          </div>
          <div className="feature-image">
             <img src="/assets/images/parti-logos.png" alt="Danske partilogoer" />
          </div>
        </section>

         <section className="feature-section">
           <div className="feature-text">
             <h2>Ugentlige Poldies</h2>
             <p>Sjovt minispil der udfordrer ens paratviden i politik</p>
             <div className="feature-buttons">
               <a href="/polidles/classic"><button className="feature-button">Klassisk</button></a>
               <a href="/polidles/image"><button className="feature-button">Billede</button></a>
             </div>
           </div>
           <div className="feature-image">
             <img src="/assets/images/polidles-game.png" alt="Polidles minispil eksempel" />
           </div>
         </section>
      </div>
    </div>
  );
};

export default HomePage;