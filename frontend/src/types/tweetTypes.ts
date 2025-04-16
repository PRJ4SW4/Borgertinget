// src/types/tweet.ts (eksempel)
export interface TweetDto {
  twitterTweetId: string;
  text: string;
  imageUrl?: string; // '?' for valgfri
  likes: number;
  retweets: number;
  replies: number;
  createdAt: string; // Datoer sendes ofte som ISO strenge
  authorName: string;
  authorHandle: string;
}

export interface PoliticianInfoDto {
  id: number; 
  name: string;
}