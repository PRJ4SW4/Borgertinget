// src/services/tweetService.ts
import {
    TweetDto,
    PoliticianInfoDto,
    PollDetailsDto,
    
} from '../types/tweetTypes'; 



interface PaginatedFeedResponse {
    tweets: TweetDto[]; 
    hasMore: boolean;
    latestPolls: PollDetailsDto[]
}



const API_BASE_URL = 'http://localhost:5218';

export { API_BASE_URL }; 

const getAuthHeaders = (): Headers => {
    const token = localStorage.getItem('jwt');
    const headers = new Headers();
    headers.set('Accept', 'application/json');
    headers.set('Content-Type', 'application/json'); 
    if (token) {
        headers.set('Authorization', `Bearer ${token}`);
    } else {
         console.warn('JWT token not found for request.');
    }
    return headers;
}

export const getFeed = async (
    page: number = 1,
    pageSize: number = 5,
    politicianId: number | null = null
): Promise<PaginatedFeedResponse> => {

  const headers = getAuthHeaders();
 
  headers.delete('Content-Type');

  try {
    let apiUrl = `${API_BASE_URL}/api/feed?page=${page}&pageSize=${pageSize}`;
    if (politicianId !== null) {
        apiUrl += `&politicianId=${politicianId}`;
    }
    console.log("Fetching feed from URL:", apiUrl);

    const response = await fetch(apiUrl, {
      method: 'GET',
      headers: headers
    });

    if (!response.ok) {
        let errorMsg = `Failed to fetch feed. Status: ${response.status}`;
        try {
            const errorData = await response.json();
            errorMsg = errorData.message || errorData.title || JSON.stringify(errorData) || errorMsg;
        } catch  { /* Ignore JSON parsing errors */ }
        if (response.status === 401) { errorMsg = 'Manglende eller ugyldig godkendelse.'; }
        throw new Error(errorMsg);
    }
    return await response.json() as PaginatedFeedResponse;

  } catch (error) {
    console.error(`Error in getFeed service (Page ${page}, Filter ${politicianId}):`, error);
    throw error;
  }
};


export const getSubscriptions = async (): Promise<PoliticianInfoDto[]> => {
    const headers = getAuthHeaders();
    headers.delete('Content-Type'); 
    if (!headers.has('Authorization')) { return []; }

    try {
        const apiUrl = `${API_BASE_URL}/api/subscriptions`;
        const response = await fetch(apiUrl, { method: 'GET', headers: headers });
        if (!response.ok)  {  throw new Error(`Failed sub fetch ${response.status}`); }
        return await response.json() as PoliticianInfoDto[];
    } catch (error) { console.error('Error in getSubscriptions:', error); throw error; }
};


export const submitVote = async (pollId: number, optionId: number): Promise<void> => {
    const headers = getAuthHeaders();
    if (!headers.has('Authorization')) {
        throw new Error('Authentication required to vote.');
    }

    try {
        const apiUrl = `${API_BASE_URL}/api/polls/${pollId}/vote`;
        const body = JSON.stringify({ optionId: optionId }); 

        console.log(`Submitting vote to ${apiUrl} with body: ${body}`);

        const response = await fetch(apiUrl, {
            method: 'POST',
            headers: headers, 
            body: body
        });

        if (!response.ok) {
            let errorMsg = `Failed to submit vote. Status: ${response.status}`;
            try {
                 const errorData = await response.json();
                 if (response.status === 409) { errorMsg = errorData.title || errorData.message || "Du har allerede stemt."; }
                 else { errorMsg = errorData.title || errorData.message || JSON.stringify(errorData) || errorMsg; }
            } catch  { console.debug('Kunne ikke parse error response som JSON'); }

            if (response.status === 401) { errorMsg = 'Manglende eller ugyldig godkendelse.'; }
            if (response.status === 404) { errorMsg = 'Afstemning ikke fundet.'; }
            if (response.status === 400) { errorMsg = errorMsg || 'Ugyldigt input.';}

            throw new Error(errorMsg);
        }
        console.log("Vote submitted successfully.");
        return;

    } catch (error) {
        console.error(`Error in submitVote service (Poll ${pollId}, Option ${optionId}):`, error);
        throw error; 
    }
};

export const subscribe = async (politicianId: number): Promise<void> => {
  const response = await fetch(`${API_BASE_URL}/api/subscription`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${localStorage.getItem('jwt')}`
    },
    body: JSON.stringify({ politicianId })
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`Kunne ikke abonnere: ${errorText}`);
  }
};

export const unsubscribe = async (politicianId: number): Promise<void> => {
  const response = await fetch(`${API_BASE_URL}/api/subscription/${politicianId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${localStorage.getItem('jwt')}`
    }
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(`Kunne ikke afmelde abonnement: ${errorText}`);
  }
};