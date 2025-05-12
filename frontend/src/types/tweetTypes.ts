// src/types/tweet.ts 
export interface TweetDto {
  twitterTweetId: string;
  text: string;
  imageUrl?: string; // '?' for valgfri
  likes: number;
  retweets: number;
  replies: number;
  createdAt: string; 
  authorName: string;
  authorHandle: string;
}

export interface PoliticianInfoDto {
  id: number; 
  name: string;
}


export interface PollOptionDto {
  id: number;
  optionText: string;
  votes: number;
}

export interface PollDetailsDto {
  id: number;
  question: string;
  createdAt: string; 
  endedAt?: string | null; 
  isActive: boolean; 

  politicianId: number;
  politicianName: string;
  politicianHandle: string;

  options: PollOptionDto[];

  currentUserVoteOptionId?: number | null;
  totalVotes: number;
}


export type FeedItem = TweetDto | PollDetailsDto;


export const isTweet = (item: FeedItem): item is TweetDto => {
  return (item as TweetDto).twitterTweetId !== undefined;
};


export const isPoll = (item: FeedItem): item is PollDetailsDto => {
  return (item as PollDetailsDto).question !== undefined && (item as PollDetailsDto).options !== undefined;
};


export interface PaginatedFeedResponse {
  feedItems: FeedItem[]; 
  hasMore: boolean;
}



export type CreatePollResponse = PollDetailsDto;


export type SubscriptionsResponse = PoliticianInfoDto[];