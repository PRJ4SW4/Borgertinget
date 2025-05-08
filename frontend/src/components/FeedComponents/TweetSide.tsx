// src/components/TweetSide.tsx
import React from 'react';
import { TweetDto } from '../../types/tweetTypes'; 
import './TweetSide.css'; 

interface Props {
  tweet: TweetDto;
}

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
    <div className="tweet-card"> {}
      <div className="tweet-header"> {}
        {}
        <div className="author-info"> {}
          <strong className="author-name">{tweet.authorName}</strong> {}
          <span className="author-handle">@{tweet.authorHandle}</span> {}
        </div>
        <small className="timestamp">{formatDateTime(tweet.createdAt)}</small>
      </div>

      <p className="tweet-text">{tweet.text}</p> {}

      {tweet.imageUrl && (
        <img
          src={tweet.imageUrl}
          alt="Tweet billede"
          className="tweet-image" 
          onError={handleImageError}
        />
      )}

      <div className="tweet-stats"> {}
        <span>Likes: {tweet.likes}</span>
        <span>Retweets: {tweet.retweets}</span>
        <span>Replies: {tweet.replies}</span>
      </div>
    </div>
  );
};

export default TweetSide;