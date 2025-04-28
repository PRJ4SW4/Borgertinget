// src/pages/FeedPage.tsx
import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import * as signalR from "@microsoft/signalr";
import { LogLevel } from "@microsoft/signalr"; // <-- Importer LogLevel
import { getFeed, getSubscriptions, submitVote, API_BASE_URL } from '../services/tweetService';
import {
    TweetDto,
    PoliticianInfoDto,
    PollDetailsDto,
    PollOptionDto // Bruges af PollCard og SignalR opdatering
    // FeedItem, isTweet, isPoll er ikke længere nødvendige her
} from '../types/tweetTypes';
import TweetSide from '../components/TweetSide';
import PollCard from '../components/Pollcard'; // Sørg for denne import er korrekt ift. filnavn casing!
import './FeedPage.css';
import './Sidebar.css';
//import './FeedItemList.css'; // Navngiv evt. om hvis den kun bruges til tweets

const FeedPage: React.FC = () => {
    // --- State (Opdelt) ---
    const [tweets, setTweets] = useState<TweetDto[]>([]);           // Kun til paginerede tweets
    const [latestPolls, setLatestPolls] = useState<PollDetailsDto[]>([]); // Kun til de nyeste polls (uden filter)
    const [page, setPage] = useState<number>(1);                   // Paginering for tweets
    const [hasMore, setHasMore] = useState<boolean>(true);         // Gælder for tweets
    const [isLoading, setIsLoading] = useState<boolean>(true);       // Generel loading for feed/tweets
    const [error, setError] = useState<string | null>(null);
    const [subscriptions, setSubscriptions] = useState<PoliticianInfoDto[]>([]); // Til sidebar
    const [selectedPoliticianId, setSelectedPoliticianId] = useState<number | null>(null);

    const connectionRef = useRef<signalR.HubConnection | null>(null);
    const navigate = useNavigate();

    // --- Intersection Observer (til tweets) ---
    const observer = useRef<IntersectionObserver | null>(null);
    const lastElementRef = useCallback((node: HTMLDivElement | null) => { // Observerer sidste TWEET
        if (isLoading) return;
        if (observer.current) observer.current.disconnect();
        observer.current = new IntersectionObserver(entries => {
            if (entries[0].isIntersecting && hasMore) { // hasMore gælder tweets
                console.log("IntersectionObserver: Last TWEET visible, loading next tweet page.");
                setPage(prevPage => prevPage + 1);
            }
        });
        if (node) observer.current.observe(node);
    }, [isLoading, hasMore]);

    // --- Effekt til Subscriptions (med ekstra logging) ---
    useEffect(() => {
        const fetchSubs = async () => {
            try {
                // *** DEBUG LOG 1 ***
                console.log("FEEDPAGE: Forsøger at hente subscriptions...");
                const subsData = await getSubscriptions();
                // *** DEBUG LOG 2 ***
                // Log den RÅ data modtaget fra API'et
                console.log("FEEDPAGE: Modtog subsData fra getSubscriptions():", JSON.stringify(subsData, null, 2));
                setSubscriptions(subsData || []); // Sørg for at sætte til tom array hvis subsData er null/undefined
                 // *** DEBUG LOG 3 ***
                // Log lige efter state er (forsøgt) sat - bemærk state opdateres asynkront!
                console.log("FEEDPAGE: setSubscriptions(subsData) er kaldt. Nuværende 'subscriptions' state vil opdateres i næste render.");
            } catch (err) {
                 // *** DEBUG LOG 4 ***
                console.error("FEEDPAGE: Fejl under hentning af subscriptions:", err);
                setSubscriptions([]); // Sæt til tom array ved fejl
            }
        };
        fetchSubs();
    }, []); // Kører kun ved mount

    // --- Effekt til Feed Data (Opdateret til at håndtere Tweets OG LatestPolls) ---
    useEffect(() => {
        if (page > 1 && !hasMore) return;
        setIsLoading(true);
        if (page === 1) setError(null);

        const fetchFeedData = async () => {
            console.log(`Workspaceing page ${page} for filter: ${selectedPoliticianId ?? 'Alle'}`); // Ændret Workspaceing
            try {
                const responseData = await getFeed(page, 5, selectedPoliticianId);
                console.log("FEEDPAGE: Modtog feed data:", responseData); // Log feed data

                // Opdater tweets state
                if (page === 1) {
                    setTweets(responseData.tweets || []);
                } else {
                    setTweets(prevTweets => [...prevTweets, ...(responseData.tweets || [])]);
                }

                // Opdater latestPolls state KUN ved side 1
                if (page === 1) {
                    setLatestPolls(responseData.latestPolls || []);
                }
                setHasMore(responseData.hasMore);

            } catch (err) {
                 console.error(`Error fetching feed page ${page}`, err);
                 if (err instanceof Error) { setError(err.message); }
                 else { setError("An unknown error occurred fetching feed"); }
                 setHasMore(false);
             } finally {
                setIsLoading(false);
            }
        };
        fetchFeedData();
    }, [page, selectedPoliticianId]);

    // --- Effekt til SignalR (Opdaterer nu latestPolls state) ---
    useEffect(() => {
        const hubUrl = `${API_BASE_URL}/feedHub`;
        console.log("Setting up SignalR connection to:", hubUrl);
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, { accessTokenFactory: () => localStorage.getItem('jwt') || "" })
            .configureLogging(signalR.LogLevel.Trace) // Sæt til Trace for max detaljer

            .withAutomaticReconnect().build();
            
        connectionRef.current = newConnection;

        newConnection.on("PollVotesUpdated", (pollId: number, updatedOptionsData: { optionId: number, votes: number }[]) => {
             console.log(`SignalR: PollVotesUpdated received for pollId: ${pollId}`, updatedOptionsData);
             setLatestPolls(currentPolls => { // ... (resten af SignalR logikken) ...
                 const updatedPolls = [...currentPolls];
                 const pollIndex = updatedPolls.findIndex(p => p.id === pollId);
                 if (pollIndex > -1) {
                     console.log(`SignalR: Found poll in latestPolls at index ${pollIndex}, updating votes...`);
                     const pollToUpdate = { ...updatedPolls[pollIndex] };
                     let totalVotes = 0;
                     const updatedOptions = pollToUpdate.options.map(option => {
                         const updateData = updatedOptionsData.find(u => u.optionId === option.id);
                         const newVotes = updateData ? updateData.votes : option.votes;
                         totalVotes += newVotes;
                         return { ...option, votes: newVotes };
                     });
                     pollToUpdate.options = updatedOptions;
                     pollToUpdate.totalVotes = totalVotes;
                     updatedPolls[pollIndex] = pollToUpdate;
                     return updatedPolls;
                 }
                 console.log(`SignalR: Poll with ID ${pollId} not found in current latestPolls state.`);
                 return currentPolls;
             });
        });

        newConnection.start()
             .then(() => console.log('SignalR Connection established.'))
             .catch(err => console.error('SignalR Connection failed: ', err));

        return () => {
            console.log("Stopping SignalR connection...");
            connectionRef.current?.stop()
                .then(() => console.log("SignalR Connection stopped."))
                .catch(err => console.error("Error stopping SignalR connection:", err));
            connectionRef.current = null;
        };
    }, []);

    // --- Filter Handler ---
    const handleFilterChange = (id: number | null) => {
        if (id === selectedPoliticianId || isLoading) return;
        setTweets([]);
        setLatestPolls([]);
        setPage(1);
        setHasMore(true);
        setSelectedPoliticianId(id);
        window.scrollTo(0, 0);
      };

      // --- Tilbage knap handler ---
      const handleBackClick = () => { navigate('/home'); };

      // --- Stemme Handler ---
      const handleVoteSubmit = async (pollId: number, optionId: number) => {
          setError(null);
          try {
                console.log(`Submitting vote via API for poll ${pollId}, option ${optionId}`);
                await submitVote(pollId, optionId);
                console.log(`Vote submitted for poll ${pollId}. Waiting for SignalR update for counts.`);
                setLatestPolls(currentPolls => { /* ... opdater currentUserVoteOptionId ... */
                     const updatedPolls = [...currentPolls];
                     const pollIndex = updatedPolls.findIndex(p => p.id === pollId);
                     if (pollIndex > -1) {
                         const pollToUpdate = {...updatedPolls[pollIndex]};
                         pollToUpdate.currentUserVoteOptionId = optionId;
                         updatedPolls[pollIndex] = pollToUpdate;
                         console.log(`Locally updated currentUserVoteOptionId for poll ${pollId} in latestPolls`);
                         return updatedPolls;
                     }
                     return currentPolls;
                 });
          } catch (error) { /* Fejlhåndtering */
                console.error("Failed to submit vote:", error);
                setError(`Kunne ikke afgive stemme: ${error instanceof Error ? error.message : 'Ukendt fejl'}`);
           }
      };

      // *** DEBUG LOG 5 ***
      // Log state LIGE FØR rendering starter
      console.log("FEEDPAGE RENDER - isLoading:", isLoading, "subscriptions:", JSON.stringify(subscriptions, null, 2));

      // --- Rendering ---
      return (
        <div className="page-layout">
          <div className="sidebar">
            <h2 className="sidebar-title">Følger</h2>
            <ul className="sidebar-nav-list">
              <li className={`sidebar-nav-item ${selectedPoliticianId === null ? 'active-filter' : ''}`}>
                <button onClick={() => handleFilterChange(null)} disabled={isLoading || selectedPoliticianId === null} className="sidebar-nav-button">Alle Tweets</button>
              </li>
              {/* --- DEBUG LOG 6 (IIFE) --- */}
              {(() => {
                  console.log("FEEDPAGE SIDEBAR RENDER: Checking subscriptions. Length:", subscriptions?.length ?? 0, "IsLoading:", isLoading); // Added nullish coalescing
                  // Tjekker BÅDE på om dataen er en array OG om den har elementer
                  if (Array.isArray(subscriptions) && subscriptions.length > 0) {
                      console.log("FEEDPAGE SIDEBAR RENDER: Mapping subscriptions...");
                      return subscriptions.map((sub) => (
                          <li key={sub.id} className={`sidebar-nav-item ${selectedPoliticianId === sub.id ? 'active-filter' : ''}`}>
                              <button onClick={() => handleFilterChange(sub.id)} disabled={isLoading || selectedPoliticianId === sub.id} className="sidebar-nav-button">{sub.name}</button>
                          </li>
                      ));
                  } else if (!isLoading) {
                      console.log("FEEDPAGE SIDEBAR RENDER: Showing 'Du følger ingen'.");
                      return <li className="sidebar-nav-item-info">Du følger ingen.</li>;
                  } else {
                      console.log("FEEDPAGE SIDEBAR RENDER: Not showing buttons or 'Du følger ingen' (isLoading or length=0).");
                      return null;
                  }
              })()}
            </ul>
            <button onClick={handleBackClick} className="back-button sidebar-back-button">&larr; Tilbage til Home</button>
          </div>

          <div className="feed-content">
            <h1 className="feed-title">Dit Feed</h1>
            {error && <div className="error-message global-error">Fejl: {error}</div>}

            {/* SEKTION FOR SENESTE POLLS */}
            {!selectedPoliticianId && latestPolls.length > 0 && (
                <div className="latest-polls-section">
                    {latestPolls.map(poll => (
                        <div key={`poll-${poll.id}`}>
                            <PollCard poll={poll} onVoteSubmit={handleVoteSubmit} />
                        </div>
                    ))}
                    <hr style={{margin: "20px 0"}} />
                </div>
            )}

            {/* SEKTION FOR TWEETS */}
            {!isLoading && tweets.length === 0 && latestPolls.length === 0 && !error && (
                 <p className="empty-feed-message">Ingen tweets eller polls at vise.</p>
            )}
             <div className="tweet-list-container">
                 {tweets.map((tweet, index) => {
                     const isLastItem = tweets.length === index + 1;
                     return (
                         <div ref={isLastItem ? lastElementRef : null} key={`tweet-${tweet.twitterTweetId}`}>
                             <TweetSide tweet={tweet} />
                         </div>
                     );
                 })}
             </div>

             {/* Loading / End messages */}
             {isLoading && <div className="loading-more-message">Henter...</div>}
             {!isLoading && !hasMore && tweets.length > 0 && <p className="end-of-feed-message">Ikke flere tweets at vise.</p>}

          </div>
        </div>
      );
    };

export default FeedPage;