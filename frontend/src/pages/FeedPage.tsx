// src/pages/FeedPage.tsx
import React, { useState, useEffect, useRef, useCallback } from 'react'; // <-- Tilføjet useRef og useCallback
// Ret stier om nødvendigt
import { getFeed } from '../services/tweetService';
import { TweetDto } from '../types/tweetTypes';
import TweetSide from '../components/TweetSide'; // Matcher dit komponentnavn
import './FeedPage.css'; // Til sidens layout

const FeedPage: React.FC = () => {
  const [tweets, setTweets] = useState<TweetDto[]>([]);
  const [page, setPage] = useState<number>(1); // Holder styr på hvilken side, der skal hentes næste gang
  const [hasMore, setHasMore] = useState<boolean>(true); // Indikerer om backend har flere sider
  const [isLoading, setIsLoading] = useState<boolean>(true); // Bruges nu til både initial load og "load more"
  const [error, setError] = useState<string | null>(null);

  // --- Intersection Observer Opsætning ---
  const observer = useRef<IntersectionObserver | null>(null);
  // useCallback bruges her til at give en stabil funktion som ref-callback
  const lastTweetElementRef = useCallback((node: HTMLDivElement | null) => {
    // Hvis vi allerede er i gang med at hente data, skal vi ikke gøre noget
    if (isLoading) return;
    // Hvis der er en tidligere observer, stopper vi den
    if (observer.current) observer.current.disconnect();

    // Opretter en ny observer
    observer.current = new IntersectionObserver(entries => {
      // entries[0] er vores 'node' (det sidste tweet element)
      // Hvis elementet er synligt på skærmen OG der er mere data at hente...
      if (entries[0].isIntersecting && hasMore) {
        // ...så sætter vi sidetallet op for at hente næste side
        setPage(prevPage => prevPage + 1);
      }
    });
    // Hvis 'node'-elementet findes i DOM, begynder vi at observere det
    if (node) observer.current.observe(node);
  }, [isLoading, hasMore]); // Observeren skal genoprettes hvis isLoading eller hasMore ændres

  // --- Datahentnings Effekt ---
  // Denne useEffect kører nu HVER gang 'page' ændres
  useEffect(() => {
    // Sæt loading til true, når vi starter med at hente (uanset sidetal)
    setIsLoading(true);
    setError(null);

    const fetchFeedData = async () => {
      try {
        // Kald service med det aktuelle sidetal og ønsket størrelse
        const responseData = await getFeed(page, 5); // Henter side 'page', 5 tweets

        // Tilføj de nye tweets til den eksisterende liste
        // Bruger en funktion i setTweets for at få adgang til den forrige state (prevTweets)
        setTweets(prevTweets => {
          // Undgå at tilføje dubletter, hvis samme data hentes igen (sjældent, men god praksis)
          const newTweets = responseData.tweets.filter(
              newTweet => !prevTweets.some(prevTweet => prevTweet.twitterTweetId === newTweet.twitterTweetId)
          );
          return [...prevTweets, ...newTweets]; // Returner gammel liste + filtreret ny liste
        });

        // Opdater om der er flere sider at hente
        setHasMore(responseData.hasMore);

      } catch (err) {
        console.error(`Error fetching feed page ${page}:`, err);
        if (err instanceof Error) { setError(err.message); }
        else { setError("An unknown error occurred"); }
        setHasMore(false); // Stop med at prøve at hente mere hvis der opstår fejl
      } finally {
        setIsLoading(false); // Stop loading når hentning er færdig (succes eller fejl)
      }
    };

    // Kald kun fetchFeedData hvis vi ved (eller antager) der er mere data
    // Ellers vil den kalde unødigt når hasMore bliver false
    if (hasMore || page === 1) { // Hent altid side 1, ellers kun hvis hasMore er true
        fetchFeedData();
    } else {
        setIsLoading(false); // Sørg for loading stopper hvis hasMore var false fra start
    }

  }, [page]); // Denne effekt er afhængig af 'page' state


  // --- Rendering ---

  // Viser kun fejl, hvis der slet ingen tweets er loaded endnu
  if (error && tweets.length === 0) {
    return <div className="error-message">Error: {error}</div>;
  }

  return (
    <div className="feed-page-container">
      <h1 className="feed-title">Dit Tweet Feed</h1>

      {/* Viser "tomt feed" kun hvis der ingen tweets er OG vi ikke loader */}
      {tweets.length === 0 && !isLoading ? (
        <p className="empty-feed-message">Dit feed er tomt. Følger du nogen politikere?</p>
      ) : (
        <div className="tweet-list-container">
          {/* Mapper over tweets og viser TweetSide for hver */}
          {tweets.map((tweet, index) => {
            // Hvis det er det sidste element i listen, tilføj ref'en til observeren
            if (tweets.length === index + 1) {
              return <div ref={lastTweetElementRef} key={tweet.twitterTweetId}><TweetSide tweet={tweet} /></div>;
            } else {
              return <TweetSide key={tweet.twitterTweetId} tweet={tweet} />;
            }
          })}

          {/* Viser en loading-indikator i bunden, mens den henter MERE data */}
          {isLoading && <div className="loading-more-message">Loading more tweets...</div>}

          {/* Viser en besked, når der ikke er flere tweets at hente */}
          {!isLoading && !hasMore && <p className="end-of-feed-message">Ikke flere tweets at vise.</p>}
        </div>
      )}
    </div>
  );
};

export default FeedPage;