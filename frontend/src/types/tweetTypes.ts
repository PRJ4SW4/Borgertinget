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

//##nedestående et nyt!! ift til rollback :)

export interface PollOptionDto {
  id: number;
  optionText: string;
  votes: number;
  // votePercentage?: number; // Kan tilføjes hvis backend beregner det
}

export interface PollDetailsDto {
  id: number;
  question: string;
  createdAt: string; // ISO date string
  endedAt?: string | null; // ISO date string or null
  isActive: boolean; // Beregnet i backend DTO

  politicianId: number;
  politicianName: string;
  politicianHandle: string;

  options: PollOptionDto[];

  currentUserVoteOptionId?: number | null; // ID på den option brugeren har stemt på
  totalVotes: number;
}

// --- FÆLLES FEED ITEM TYPE (Union Type) ---
// Gør det nemmere at have en liste med både tweets og polls
export type FeedItem = TweetDto | PollDetailsDto;

// --- TYPE GUARD (til at skelne mellem typer i rendering) ---
// Tjekker om et FeedItem er en TweetDto baseret på en unik property
export const isTweet = (item: FeedItem): item is TweetDto => {
  return (item as TweetDto).twitterTweetId !== undefined;
};

// Tjekker om et FeedItem er en PollDetailsDto
export const isPoll = (item: FeedItem): item is PollDetailsDto => {
  return (item as PollDetailsDto).question !== undefined && (item as PollDetailsDto).options !== undefined;
};


export interface PaginatedFeedResponse {
  feedItems: FeedItem[]; 
  hasMore: boolean;
}



export type CreatePollResponse = PollDetailsDto;


export type SubscriptionsResponse = PoliticianInfoDto[];