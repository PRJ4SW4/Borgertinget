// src/services/tweetService.ts
// Ret sti til din type-fil hvis nødvendigt
import { TweetDto } from '../types/tweetTypes';

// Definer en interface for det objekt backend nu returnerer
interface PaginatedFeedResponse {
  tweets: TweetDto[];
  hasMore: boolean;
  // Tilføj evt. andre felter backend sender med (totalCount etc.)
}

// Din backend URL
const API_BASE_URL = 'http://localhost:5218';

// Opdater funktionen til at acceptere parametre og returnere det nye format
export const getFeed = async (page: number = 1, pageSize: number = 5): Promise<PaginatedFeedResponse> => {

  // Hent token (som før)
  const token = localStorage.getItem('jwt');
  const headers: HeadersInit = {
    'Accept': 'application/json',
    'Content-Type': 'application/json'
  };
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  } else {
    console.warn('JWT token not found for feed request.');
  }

  try {
    // Tilføj page og pageSize til URL'en som query parametre
    const apiUrl = `${API_BASE_URL}/api/feed?page=${page}&pageSize=${pageSize}`;

    // Lav fetch kaldet med headers
    const response = await fetch(apiUrl, {
      method: 'GET',
      headers: headers
    });

    // Fejlhåndtering (som før, tjek response.ok etc.)
    if (!response.ok) {
      let errorMsg = `Failed to fetch feed (Page ${page}). Status: ${response.status}`;
       try { const errorData = await response.json(); errorMsg = errorData.message || errorData.title || errorMsg; } catch(e) {}
       if (response.status === 401) { errorMsg = 'Authentication failed.'; /* Håndter logud? */ }
      throw new Error(errorMsg);
    }

    // Parse JSON og returner HELE objektet (med typen PaginatedFeedResponse)
    return await response.json() as PaginatedFeedResponse;

  } catch (error) {
    console.error(`Error in getFeed service (Page ${page}):`, error);
    throw error; // Kast videre til FeedPage
  }
};