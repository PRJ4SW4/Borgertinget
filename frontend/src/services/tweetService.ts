// src/services/feedService.ts (eksempel)
import { TweetDto } from '../types/tweetTypes.ts';
const API_BASE_URL = 'http://localhost:5218';

export const getFeed = async (): Promise<TweetDto[]> => {
  const response = await fetch(`${API_BASE_URL}/api/feed`);
  if (!response.ok) {
    throw new Error('Failed to fetch feed');
  }
  return await response.json() as TweetDto[];
};