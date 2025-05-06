// src/pages/FeedPage.tsx
import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import * as signalR from "@microsoft/signalr";
import { getFeed, getSubscriptions, submitVote, API_BASE_URL } from '../services/tweetService';
import {
    TweetDto,
    PoliticianInfoDto,
    PollDetailsDto,
} from '../types/tweetTypes';
import TweetSide from '../components/FeedComponents/TweetSide';
import PollCard from '../components/FeedComponents/Pollcard';
import Sidebar from '../components/FeedComponents/Sidebar';
import './FeedPage.css';

const FeedPage: React.FC = () => {
    const [tweets, setTweets] = useState<TweetDto[]>([]);
    const [latestPolls, setLatestPolls] = useState<PollDetailsDto[]>([]);
    const [page, setPage] = useState<number>(1);
    const [hasMore, setHasMore] = useState<boolean>(true);
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);
    const [subscriptions, setSubscriptions] = useState<PoliticianInfoDto[]>([]);
    const [selectedPoliticianId, setSelectedPoliticianId] = useState<number | null>(null);

    const connectionRef = useRef<signalR.HubConnection | null>(null);
    const navigate = useNavigate();

    const observer = useRef<IntersectionObserver | null>(null);
    const lastElementRef = useCallback((node: HTMLDivElement | null) => {
        if (isLoading) return;
        if (observer.current) observer.current.disconnect();
        observer.current = new IntersectionObserver(entries => {
            if (entries[0].isIntersecting && hasMore) {
                console.log("IntersectionObserver: Last TWEET visible, loading next tweet page.");
                setPage(prevPage => prevPage + 1);
            }
        });
        if (node) observer.current.observe(node);
    }, [isLoading, hasMore]);

    useEffect(() => {
        const fetchSubs = async () => {
            try {
                console.log("FEEDPAGE: Forsøger at hente subscriptions...");
                const subsData = await getSubscriptions();
                console.log("FEEDPAGE: Modtog subsData fra getSubscriptions():", JSON.stringify(subsData, null, 2));
                setSubscriptions(subsData || []);
                console.log("FEEDPAGE: setSubscriptions(subsData) er kaldt. Nuværende 'subscriptions' state vil opdateres i næste render.");
            } catch (err) {
                console.error("FEEDPAGE: Fejl under hentning af subscriptions:", err);
                setSubscriptions([]);
            }
        };
        fetchSubs();
    }, []);

    useEffect(() => {
        if (page > 1 && !hasMore) return;
        setIsLoading(true);
        if (page === 1) setError(null);

        const fetchFeedData = async () => {
            console.log(`Fetching page ${page} for filter: ${selectedPoliticianId ?? 'Alle'}`);
            try {
                const responseData = await getFeed(page, 5, selectedPoliticianId);
                console.log("FEEDPAGE: Modtog feed data:", responseData);

                if (page === 1) {
                    setTweets(responseData.tweets || []);
                } else {
                    setTweets(prevTweets => [...prevTweets, ...(responseData.tweets || [])]);
                }

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

    useEffect(() => {
        const hubUrl = `${API_BASE_URL}/feedHub`;
        console.log("Setting up SignalR connection to:", hubUrl);
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, { accessTokenFactory: () => localStorage.getItem('jwt') || "" })
            .configureLogging(signalR.LogLevel.Trace)
            .withAutomaticReconnect().build();
            
        connectionRef.current = newConnection;

        newConnection.on("PollVotesUpdated", (pollId: number, updatedOptionsData: { optionId: number, votes: number }[]) => {
            console.log(`SignalR: PollVotesUpdated received for pollId: ${pollId}`, updatedOptionsData);
            setLatestPolls(currentPolls => {
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

    const handleFilterChange = (id: number | null) => {
        if (id === selectedPoliticianId || isLoading) return;
        setTweets([]);
        setLatestPolls([]);
        setPage(1);
        setHasMore(true);
        setSelectedPoliticianId(id);
        window.scrollTo(0, 0);
    };

    const handleBackClick = () => { navigate('/home'); };

    const handleVoteSubmit = async (pollId: number, optionId: number) => {
        setError(null);
        try {
            console.log(`Submitting vote via API for poll ${pollId}, option ${optionId}`);
            await submitVote(pollId, optionId);
            console.log(`Vote submitted for poll ${pollId}. Waiting for SignalR update for counts.`);
            setLatestPolls(currentPolls => {
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
        } catch (error) {
            console.error("Failed to submit vote:", error);
            setError(`Kunne ikke afgive stemme: ${error instanceof Error ? error.message : 'Ukendt fejl'}`);
        }
    };

    return (
        <div className="page-layout">
            <Sidebar
                subscriptions={subscriptions}
                selectedPoliticianId={selectedPoliticianId}
                isLoading={isLoading}
                onFilterChange={handleFilterChange}
                onBackClick={handleBackClick}
            />
            <div className="feed-content">
                {error && <div className="error-message global-error">Fejl: {error}</div>}

                <div className="feed-actual-content">
                    {/* SEKTION FOR SENESTE POLLS */}
                    {!selectedPoliticianId && latestPolls.length > 0 && (
                        <div className="latest-polls-section">
                            <h2 className="section-title">Seneste Afstemninger</h2>
                            <div className="polls-container">
                                {latestPolls.map(poll => (
                                    <div key={`poll-${poll.id}`} className="poll-item">
                                        <PollCard poll={poll} onVoteSubmit={handleVoteSubmit} />
                                    </div>
                                ))}
                            </div>
                            <hr className="section-divider" />
                        </div>
                    )}

                    {/* SEKTION FOR TWEETS */}
                    {tweets.length > 0 && (
            <>
                <h2 className="section-title">Seneste Tweets</h2>
                <div className="tweet-list-container">
                    {tweets.map((tweet, index) => {
                        const isLastItem = tweets.length === index + 1;
                        return (
                            <div 
                                ref={isLastItem ? lastElementRef : null} 
                                key={`tweet-${tweet.twitterTweetId}`}
                                className="tweet-item"
                            >
                                <TweetSide tweet={tweet} />
                            </div>
                        );
                    })}
                </div>
            </>
                    )}

                    {!isLoading && tweets.length === 0 && latestPolls.length === 0 && !error && (
                        <p className="empty-feed-message">Ingen tweets eller polls at vise.</p>
                    )}
                </div>

                {/* Loading / End messages */}
                {isLoading && (page > 1 || (latestPolls.length > 0 || tweets.length > 0)) && 
                    <div className="loading-more-message">Henter...</div>}
                
                {isLoading && page === 1 && tweets.length === 0 && latestPolls.length === 0 && 
                    <div className="loading-more-message">Henter feed...</div>}
                
                {!isLoading && !hasMore && tweets.length > 0 && 
                    <p className="end-of-feed-message">Ikke flere tweets at vise.</p>}
            </div>
        </div>
    );
};

export default FeedPage;