// src/pages/FeedPage.tsx
import React, { useState, useEffect } from 'react';
import { getFeed } from '../services/tweetService'; // Ret sti om nødvendigt
import { TweetDto } from '../types/tweetTypes';    // Ret sti om nødvendigt
import TweetCard from '../components/TweetSide'; // <-- IMPORTER TweetCard HER
import './FeedPage.css'; // <-- ÆNDRET IMPORT

const FeedPage: React.FC = () => {
  const [tweets, setTweets] = useState<TweetDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchFeedData = async () => {
      try {
        setIsLoading(true);
        setError(null);
        const data = await getFeed();
        setTweets(data);
      } catch (err) {
        console.error("Error fetching feed:", err);
        if (err instanceof Error) {
          setError(err.message);
        } else {
          setError("An unknown error occurred while fetching the feed.");
        }
      } finally {
        setIsLoading(false);
      }
    };

    fetchFeedData();
  }, []); // Kører kun én gang

  // Vis loading eller fejl hvis relevant
  if (isLoading) {
    return <div>Loading tweets...</div>;
  }
  if (error) {
    return <div style={{ color: 'red' }}>Error: {error}</div>;
  }

  // Vis feedet ved at mappe over tweets og rendere TweetCard for hver
  return (
    <div>
        <h1 className="feed-title">Dit Tweet Feed</h1>

      {tweets.length === 0 ? (
        <p>Dit feed er tomt. Følger du nogen politikere?</p>
      ) : (
        <div>
          {tweets.map((tweet) => (
            
            <TweetCard key={tweet.twitterTweetId} tweet={tweet} />
          ))}
        </div>
      )}
    </div>
  );
};

export default FeedPage;