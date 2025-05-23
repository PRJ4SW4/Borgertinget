import { useState, useEffect } from 'react';
import { NavLink } from 'react-router-dom';
import { fetchFlashcardCollections } from '../../services/ApiService';
import type { FlashcardCollectionSummaryDto } from '../../types/flashcardTypes';
import './FlashcardSideNav.css';

function FlashcardSideNav() {
  // State for storing the list of collections
  const [collections, setCollections] = useState<FlashcardCollectionSummaryDto[]>([]);
  // State for tracking loading status
  const [isLoading, setIsLoading] = useState<boolean>(true);
  // State for storing potential errors
  const [error, setError] = useState<string | null>(null);

  // Fetch data when the component mounts
  useEffect(() => {
    const loadCollections = async () => {
      try {
        setIsLoading(true); // Start loading
        setError(null);     // Clear previous errors
        const data = await fetchFlashcardCollections();
        setCollections(data); // Store fetched data in state
      } catch (err) {
        // Handle errors during fetch
        setError(err instanceof Error ? err.message : "Failed to load flashcard collections");
        console.error("FlashcardSideNav Error loading collections:", err);
      } finally {
        setIsLoading(false); // Finish loading, regardless of success/error
      }
    };
    loadCollections();
  }, []); // Empty dependency array ensures this runs only once on mount

  // --- Render based on state ---
  if (isLoading) {
      return <nav className="flashcard-side-nav loading">Indlæser Samlinger...</nav>;
  }

  if (error) {
      return <nav className="flashcard-side-nav error">Fejl: {error}</nav>;
  }

  if (collections.length === 0) {
      return <nav className="flashcard-side-nav empty">Ingen samlinger fundet.</nav>;
  }

  // Render the list if data is loaded successfully
  return (
    <nav className="flashcard-side-nav">
      <ul>
        {/* Map over the fetched collections */}
        {collections.map(collection => (
          <li key={collection.collectionId}>
            {/* Use NavLink for automatic active class styling */}
            <NavLink
              to={`/flashcards/${collection.collectionId}`} // Link to the specific viewer route
              // The 'className' prop can accept a function to check if the link is active
              className={({ isActive }) => isActive ? 'active-nav-link' : ''}
            >
              {collection.title}
            </NavLink>
          </li>
        ))}
      </ul>
    </nav>
  );
}

export default FlashcardSideNav;