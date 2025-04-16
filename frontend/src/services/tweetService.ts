// src/services/tweetService.ts
import { TweetDto, PoliticianInfoDto } from '../types/tweetTypes'; // Sørg for stien er korrekt

interface PaginatedFeedResponse {
  tweets: TweetDto[];
  hasMore: boolean;
}

const API_BASE_URL = 'http://localhost:5218';

// --- HJÆLPEFUNKTION TIL HEADERS (Returnerer nu et Headers objekt) ---
const getAuthHeaders = (): Headers => { // <--- Ændret returtype
    const token = localStorage.getItem('jwt');
    const headers = new Headers(); // <--- Opret Headers objekt

    headers.set('Accept', 'application/json'); // <--- Brug .set()

    if (token) {
        headers.set('Authorization', `Bearer ${token}`); // <--- Brug .set()
    } else {
         console.warn('JWT token not found for request.');
    }
    return headers;
}

// --- Opdateret getFeed ---
export const getFeed = async (
    page: number = 1,
    pageSize: number = 5,
    politicianId: number | null = null
): Promise<PaginatedFeedResponse> => {

  // Få Headers objektet
  const headers = getAuthHeaders();

  // Valgfrit: Tjek om login er påkrævet for feedet
  // if (!headers.has('Authorization')) { // <-- Brug .has()
  //     return Promise.reject(new Error('Not authenticated'));
  // }

  try {
    let apiUrl = `${API_BASE_URL}/api/feed?page=${page}&pageSize=${pageSize}`;
    if (politicianId !== null) {
        apiUrl += `&politicianId=${politicianId}`;
    }
    console.log("Fetching feed from URL:", apiUrl);

    const response = await fetch(apiUrl, {
      method: 'GET',
      headers: headers // <--- Send Headers objektet direkte med
    });

    if (!response.ok) {
        let errorMsg = `Failed to fetch feed. Status: ${response.status}`;
         try {
             const errorData = await response.json();
             errorMsg = errorData.message || errorData.title || JSON.stringify(errorData) || errorMsg;
         } catch (e) {}
        if (response.status === 401) { errorMsg = 'Manglende eller ugyldig godkendelse.'; }
        throw new Error(errorMsg);
    }
    return await response.json() as PaginatedFeedResponse;
  } catch (error) {
    console.error(`Error in getFeed service (Page ${page}, Filter ${politicianId}):`, error);
    throw error;
  }
};


// --- Opdateret getSubscriptions funktion ---
export const getSubscriptions = async (): Promise<PoliticianInfoDto[]> => {
    // Få Headers objektet
    const headers = getAuthHeaders();

    // Brug .has() til at tjekke om Authorization header findes
    if (!headers.has('Authorization')) { // <--- Rettet linje
        console.warn('Not authenticated, returning empty subscriptions.');
        return [];
    }

    try {
        const apiUrl = `${API_BASE_URL}/api/subscriptions`;
        console.log("Fetching subscriptions from URL:", apiUrl);

        const response = await fetch(apiUrl, {
            method: 'GET',
            headers: headers // <--- Send Headers objektet direkte med
        });

        if (!response.ok) {
            let errorMsg = `Failed to fetch subscriptions. Status: ${response.status}`;
             try {
                 const errorData = await response.json();
                 errorMsg = errorData.message || errorData.title || JSON.stringify(errorData) || errorMsg;
             } catch (e) {}
            if (response.status === 401) { errorMsg = 'Manglende eller ugyldig godkendelse ved hentning af abonnementer.'; }
            throw new Error(errorMsg);
        }
        return await response.json() as PoliticianInfoDto[];
    } catch (error) {
        console.error('Error in getSubscriptions service:', error);
        return [];
        // throw error; // Alternativt
    }
};