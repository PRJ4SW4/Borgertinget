// src/components/PageContent.tsx
import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import ReactMarkdown from 'react-markdown'; // Ensure types are installed if needed (@types/react-markdown)
import { fetchPageDetails } from '../services/ApiService';
import type { PageDetailDto } from '../types/pageTypes'; // Import type
// import './PageContent.css';

// Type for URL parameters provided by React Router
type PageParams = {
  pageId: string; // pageId from the route ':pageId' will be a string
};

function PageContent() { // Return type is implicitly JSX.Element
  // Type the useParams hook
  const { pageId } = useParams<PageParams>();

  // Type the component's state
  const [pageDetails, setPageDetails] = useState<PageDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false); // Initial state is not loading
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!pageId) {
        setPageDetails(null); // Clear content if navigating away or no ID
        setIsLoading(false);
        setError(null);
        return; // Exit effect if no pageId
    }

    const loadPage = async () => {
      setIsLoading(true);
      setError(null);
      try {
        // pageId from useParams is a string, ensure your API can handle it or parse it
        const details = await fetchPageDetails(pageId);
        setPageDetails(details);
      } catch (err) {
        if (err instanceof Error) { // Type check error
            setError(err.message);
        } else {
            setError("An unknown error occurred loading the page");
        }
        console.error(`Failed to load page ${pageId}:`, err);
        setPageDetails(null);
      } finally {
        setIsLoading(false);
      }
    };

    loadPage();
  }, [pageId]); // Dependency array includes pageId

  if (isLoading) return <div>Indlæser sideindhold...</div>; // Loading state
  if (error) return <div style={{ color: 'red' }}>Fejl ved indlæsning af side: {error}</div>; // Error state
  // Handle the case where pageId is undefined/null (e.g., on the index route)
  if (!pageId) return <div>Vælg venligst en side fra navigationen.</div>;
  // Handle page not found (API returned null)
  if (!isLoading && !error && !pageDetails) return <div>Siden blev ikke fundet (404).</div>;
  // Handle successful load but empty details (shouldn't happen if API returns null for 404)
  if (!pageDetails) return <div>Indhold ikke tilgængeligt.</div>

  // Render content when successfully loaded
  return (
    <article>
      <h1>{pageDetails.title}</h1>
      {/* Render the Markdown fetched from the backend */}
      {/* Ensure ReactMarkdown component handles 'children' prop correctly */}
      <ReactMarkdown>{pageDetails.content}</ReactMarkdown>

      {/* Add Next/Previous buttons here later */}
    </article>
  );
}

export default PageContent;