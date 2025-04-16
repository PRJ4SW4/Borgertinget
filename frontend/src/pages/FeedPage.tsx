// src/pages/FeedPage.tsx
import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router-dom'; // <-- Tilføjet useNavigate
// Ret stier om nødvendigt
import { getFeed, getSubscriptions } from '../services/tweetService'; // <-- Tilføjet getSubscriptions
import { TweetDto, PoliticianInfoDto } from '../types/tweetTypes'; // <-- Tilføjet PoliticianInfoDto
import TweetSide from '../components/TweetSide';
import './FeedPage.css'; // Til sidens layout
import './Sidebar.css'; // <-- NY CSS fil til sidebar (se nedenfor)

const FeedPage: React.FC = () => {
  const [tweets, setTweets] = useState<TweetDto[]>([]);
  const [page, setPage] = useState<number>(1);
  const [hasMore, setHasMore] = useState<boolean>(true);
  const [isLoading, setIsLoading] = useState<boolean>(true); // Bruges til ALLE loads
  const [error, setError] = useState<string | null>(null);

  // --- NY State til Sidebar og Filter ---
  const [subscriptions, setSubscriptions] = useState<PoliticianInfoDto[]>([]);
  const [selectedPoliticianId, setSelectedPoliticianId] = useState<number | null>(null); // null = Alle

  const navigate = useNavigate(); // Til tilbage-knap

  // --- Intersection Observer (som før) ---
  const observer = useRef<IntersectionObserver | null>(null);
  const lastTweetElementRef = useCallback((node: HTMLDivElement | null) => {
    if (isLoading) return;
    if (observer.current) observer.current.disconnect();
    observer.current = new IntersectionObserver(entries => {
      if (entries[0].isIntersecting && hasMore) {
        setPage(prevPage => prevPage + 1); // Trigger fetch af næste side
      }
    });
    if (node) observer.current.observe(node);
  }, [isLoading, hasMore]);

  // --- Effekt til at hente abonnementer (kun én gang) ---
  useEffect(() => {
    const fetchSubs = async () => {
        try {
            // console.log("Fetching subscriptions...");
            const subsData = await getSubscriptions();
            setSubscriptions(subsData);
            // console.log("Subscriptions fetched:", subsData);
        } catch (err) {
             console.error("Could not fetch subscriptions:", err);
             // Vis evt. fejl specifikt for abonnementer
        }
    };
    fetchSubs();
  }, []); // Kører kun ved mount

  // --- Effekt til at hente TWEET data ---
  // Kører når 'page' ELLER 'selectedPoliticianId' ændres
  useEffect(() => {
    // Undgå at fetche side 1 unødigt hvis filter ændres FØR side 1 er loadet færdigt
    // Hvis page=1, så er det enten initial load eller et filter-reset, så lad den køre.
    // Hvis page > 1, er det infinite scroll, som KUN skal køre hvis hasMore er true.
    if (page > 1 && !hasMore) {
         // console.log("Skipping fetch: Page > 1 and !hasMore");
         return; // Gør intet hvis vi scroller, men der ikke er mere
    }

    setIsLoading(true);
    setError(null); // Nulstil fejl ved ny fetch

    const fetchFeedData = async () => {
      // console.log(`Workspaceing page ${page} for filter: ${selectedPoliticianId ?? 'Alle'}`);
      try {
        // Kald service med det aktuelle page, pageSize og filter
        const responseData = await getFeed(page, 5, selectedPoliticianId); // pageSize=5

        // Hvis det er side 1 (enten initial load eller filter ændring), ERSTAT tweets
        if (page === 1) {
            // console.log("Setting initial tweets for page 1/filter change");
            setTweets(responseData.tweets);
        } else {
        // Hvis det er side > 1 (infinite scroll), TILFØJ tweets
            // console.log("Appending tweets for page > 1");
            setTweets(prevTweets => {
              const existingIds = new Set(prevTweets.map(t => t.twitterTweetId));
              const newTweets = responseData.tweets.filter(t => !existingIds.has(t.twitterTweetId));
              return [...prevTweets, ...newTweets];
            });
        }

        setHasMore(responseData.hasMore);
        // console.log("Fetch complete. HasMore:", responseData.hasMore);

      } catch (err) {
        console.error(`Error fetching feed page ${page} for filter ${selectedPoliticianId}:`, err);
        if (err instanceof Error) { setError(err.message); }
        else { setError("An unknown error occurred"); }
        setHasMore(false); // Stop med at prøve ved fejl
      } finally {
        setIsLoading(false);
      }
    };

    fetchFeedData();

  }, [page, selectedPoliticianId]); // <-- Afhængig af BÅDE page OG filter

  // --- Klik-håndtering for Filter i Sidebar ---
  const handleFilterChange = (id: number | null) => {
    // console.log(`Filter change requested for ID: ${id}, current: ${selectedPoliticianId}`);
    if (id === selectedPoliticianId || isLoading) {
         // console.log("Filter change ignored (same filter or loading)");
         return; // Gør intet hvis samme filter vælges eller den loader
    }
    // console.log("Applying filter:", id);
    setTweets([]); // Nulstil tweet-listen
    setPage(1); // Gå ALTID tilbage til side 1 ved filterskift
    setHasMore(true); // Antag der er data for det nye filter (backend retter hvis ikke)
    setSelectedPoliticianId(id); // Sæt det nye filter (dette trigger useEffect ovenfor)
    window.scrollTo(0, 0); // Scroll til toppen af siden
  };

   // --- Klik-håndtering for Tilbage Knap ---
    const handleBackClick = () => {
        navigate('/home'); // Antager du har en /home route
    };

  // --- Rendering ---
  return (
    // Brug en layout-container til sidebar + content
    <div className="page-layout"> {/* Matcher Sidebar.css */}

      {/* --- Sidebar --- */}
      <div className="sidebar">
        <h2 className="sidebar-title">Følger</h2>
        <ul className="sidebar-nav-list">
          {/* 'Alle Tweets' Knap */}
          <li className={`sidebar-nav-item ${selectedPoliticianId === null ? 'active-filter' : ''}`}>
            <button
              onClick={() => handleFilterChange(null)}
              disabled={isLoading || selectedPoliticianId === null}
              className="sidebar-nav-button"
            >
              Alle Tweets
            </button>
          </li>

          {/* Liste over abonnementer */}
          {subscriptions.length > 0 ? (
               subscriptions.map((sub) => (
                  <li
                      key={sub.id}
                      className={`sidebar-nav-item ${selectedPoliticianId === sub.id ? 'active-filter' : ''}`}
                  >
                      <button
                          onClick={() => handleFilterChange(sub.id)}
                          disabled={isLoading || selectedPoliticianId === sub.id}
                          className="sidebar-nav-button"
                      >
                          {sub.name}
                      </button>
                  </li>
               ))
           ) : (
                // Vis besked hvis ingen abonnementer (efter fetch er forsøgt)
                !isLoading && <li className="sidebar-nav-item-info">Du følger ingen.</li>
           )}
        </ul>
         {/* Tilbage Knap */}
        <button onClick={handleBackClick} className="back-button sidebar-back-button">
            &larr; Tilbage til Home
        </button>
      </div>

      {/* --- Main Feed Content --- */}
      <div className="feed-content"> {/* Matcher Sidebar.css */}
        <h1 className="feed-title">Dit Tweet Feed</h1>

         {/* Fejl-visning (kun hvis ingen tweets er loadet endnu) */}
         {error && tweets.length === 0 && (
             <div className="error-message">Fejl ved hentning af feed: {error}</div>
         )}

         {/* Tomt feed besked (hvis ingen tweets OG ikke loading OG ingen fejl) */}
         {!isLoading && !error && tweets.length === 0 && (
              <p className="empty-feed-message">Ingen tweets at vise for det valgte filter.</p>
         )}

         {/* Tweet Liste */}
         <div className="tweet-list-container">
             {tweets.map((tweet, index) => {
                 // Tilføj ref til det sidste element for IntersectionObserver
                 if (tweets.length === index + 1) {
                     return <div ref={lastTweetElementRef} key={tweet.twitterTweetId}><TweetSide tweet={tweet} /></div>;
                 } else {
                     return <TweetSide key={tweet.twitterTweetId} tweet={tweet} />;
                 }
             })}
         </div>

          {/* Loading indikator for NÆSTE side (kun hvis loading OG der er tweets) */}
         {isLoading && tweets.length > 0 && <div className="loading-more-message">Henter flere tweets...</div>}

          {/* Slut på feed besked */}
         {!isLoading && !hasMore && tweets.length > 0 && <p className="end-of-feed-message">Ikke flere tweets at vise.</p>}

      </div> {/* Slut på feed-content */}

    </div> // Slut på page-layout
  );
};

export default FeedPage;