// src/components/TweetSide.tsx
import React from 'react';
import { TweetDto } from '../types/tweetTypes'; // Ret sti om nødvendigt

// Importer CSS filen direkte - dette gør stylingen global
import './TweetSide.css'; // <-- ÆNDRET IMPORT

interface Props {
  tweet: TweetDto;
}

// Omdøbt komponentnavn så det passer til filnavnet
const TweetSide: React.FC<Props> = ({ tweet }) => {
  const formatDateTime = (dateString: string) => {
      try {
          return new Date(dateString).toLocaleString('da-DK', { dateStyle: 'short', timeStyle: 'short' });
      } catch (e) {
          console.error("Error formatting date:", dateString, e);
          return "Ugyldig dato";
      }
  };

  const handleImageError = (event: React.SyntheticEvent<HTMLImageElement, Event>) => {
      event.currentTarget.style.display = 'none';
      console.error("Failed to load image:", tweet.imageUrl);
  };

  return (
    // Brug klassenavne som strenge direkte fra CSS-filen
    <div className="tweet-card"> {/* <-- ÆNDRET className */}
      <div className="tweet-header"> {/* <-- ÆNDRET className */}
        {/* Placeholder for profile picture */}
        <div className="author-info"> {/* <-- ÆNDRET className */}
          <strong className="author-name">{tweet.authorName}</strong> {/* <-- ÆNDRET className */}
          <span className="author-handle">@{tweet.authorHandle}</span> {/* <-- ÆNDRET className */}
        </div>
        <small className="timestamp">{formatDateTime(tweet.createdAt)}</small>
      </div>

      <p className="tweet-text">{tweet.text}</p> {/* <-- ÆNDRET className */}

      {tweet.imageUrl && (
        <img
          src={tweet.imageUrl}
          alt="Tweet billede"
          className="tweet-image" // <-- ÆNDRET className
          onError={handleImageError}
        />
      )}

      <div className="tweet-stats"> {/* <-- ÆNDRET className */}
        <span>Likes: {tweet.likes}</span>
        <span>Retweets: {tweet.retweets}</span>
        <span>Replies: {tweet.replies}</span>
      </div>
    </div>
  );
};

// Omdøbt export så det passer til filnavnet
export default TweetSide;