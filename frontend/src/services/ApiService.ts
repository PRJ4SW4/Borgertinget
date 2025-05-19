import type { PageSummaryDto, PageDetailDto } from '../types/pageTypes'; // Import types
import type { FlashcardCollectionSummaryDto, FlashcardCollectionDetailDto } from '../types/flashcardTypes';
import type { CalendarEventDto } from '../types/calendarTypes';

const API_BASE_URL = '/api'; // Use relative path for API calls, vite.config.ts handles proxying

// Helper function for getting authorization headers
const getAuthHeaders = (includeContentType = true): Headers => {
    const token = localStorage.getItem('jwt');
    const headers = new Headers();
    headers.set('Accept', 'application/json');

    if (includeContentType) {
        headers.set('Content-Type', 'application/json');
    }

    if (token) {
        const cleanToken = token.replace(/^["'](.*)["']$/, '$1');
        headers.set('Authorization', `Bearer ${cleanToken}`);
    } 
    return headers;
};

interface AnswerCheckRequest {
  questionId: number;
  selectedAnswerOptionId: number;
}

export interface AnswerCheckResponse { 
  isCorrect: boolean;
}

export const checkAnswer =
    async(payload: AnswerCheckRequest): Promise<AnswerCheckResponse> => {
  const headers = getAuthHeaders(); // Use helper

  const response = await fetch(`${API_BASE_URL}/answers/check`, { // Ensure API_BASE_URL is used
    // Match backend route
    method: 'POST',
    headers: headers,
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    // Handle API errors (like BadRequest from backend)
    const errorText = await response.text(); // Get error details if possible
    console.error("API Error:", response.status, errorText);
    throw new Error(`Failed to check answer (${response.status})`);
  }
  return await response.json() as AnswerCheckResponse;
};

export const fetchPagesStructure = async (): Promise<PageSummaryDto[]> => {
  const headers = getAuthHeaders(false); // Use helper, no content-type for GET

  const response = await fetch(`${API_BASE_URL}/pages/structure`, {
    headers: headers,
  });
  if (!response.ok) {
    throw new Error(`Failed to fetch page structure: ${response.statusText}`);
  }
  // Explicitly type the response parsing
  return await response.json() as PageSummaryDto[];
};

export const fetchPageDetails =
    async(id: string|number): Promise<PageDetailDto|null> => {
  if (!id) return null;

  const headers = getAuthHeaders(false); // Use helper, no content-type for GET

  const response = await fetch(`${API_BASE_URL}/pages/${id}`, {
    headers: headers,
  });
  if (response.status === 404) {
    return null;  // Page not found
  }
  if (!response.ok) {
    throw new Error(
        `Failed to fetch page details for ID ${id}: ${response.statusText}`);
  }
  return await response.json() as PageDetailDto;
};


// --- Flashcard Functions ---

export const fetchFlashcardCollections =
    async(): Promise<FlashcardCollectionSummaryDto[]> => {
  const headers = getAuthHeaders(false); // Use helper, no content-type for GET

  const response = await fetch(`${API_BASE_URL}/flashcards/collections`, {
    headers: headers,
  });
  if (!response.ok) {
    console.error('API Error:', response.status, await response.text());
    throw new Error('Failed to fetch flashcard collections');
  }
  return await response.json() as FlashcardCollectionSummaryDto[];
};

export const fetchFlashcardCollectionDetails =
    async(collectionId: string|
          number): Promise<FlashcardCollectionDetailDto|null> => {
  if (!collectionId || collectionId === 'undefined' ||
      collectionId === 'null') {
    console.warn(
        'fetchFlashcardCollectionDetails called with invalid ID:',
        collectionId);
    return null;
  }

  const headers = getAuthHeaders(false); // Use helper, no content-type for GET

  const response =
      await fetch(`${API_BASE_URL}/flashcards/collections/${collectionId}`, {
        headers: headers,
      });

  if (response.status === 404) {
    console.warn(`Flashcard collection not found: ID ${collectionId}`);
    return null;  // Return null if collection doesn't exist
  }
  if (!response.ok) {
    console.error('API Error:', response.status, await response.text());
    throw new Error(
        `Failed to fetch details for flashcard collection ${collectionId}`);
  }
  return await response.json() as FlashcardCollectionDetailDto;
};

// --- Calendar Functions ---
export const fetchCalendarEvents = async (): Promise<CalendarEventDto[]> => {
  const headers = getAuthHeaders(false); // Use helper, no content-type for GET
  const url = `${API_BASE_URL}/calendar/events`;
  console.log("Fetching calendar events from:", url); // For debugging

  const response = await fetch(url, {
      method: 'GET',
      headers: headers,
  });

  if (!response.ok) {
    const errorText = await response.text();
    console.error(
        'API Error fetching calendar events:', response.status, errorText);
    throw new Error(`Failed to fetch calendar events (${response.status})`);
  }
  return await response.json() as CalendarEventDto[];
};

export async function toggleEventInterest(eventId: number): Promise<{ isInterested: boolean, interestedCount: number }> {
  const headers = getAuthHeaders();
  if (!headers.has('Authorization')) {
    throw new Error('Authentication required to toggle event interest.');
  }

  try {
    const response = await fetch(`${API_BASE_URL}/calendar/events/toggle-interest/${eventId}`, {
      method: 'POST',
      headers: headers,
      body: JSON.stringify({}),
    });

    if (!response.ok) {
      // Attempt to parse error response for better message
      let errorMsg = `API request failed with status ${response.status}`;
      try {
        const errorData = await response.json();
        errorMsg = errorData.message || errorData.title || JSON.stringify(errorData) || errorMsg;
      } catch { /* Ignore JSON parsing errors, use status code based message */ }
      throw new Error(errorMsg);
    }
    return await response.json();

  } catch (error: unknown) {
    console.error('Error in toggleEventInterest:', error);
    // Ensure the caught error is re-thrown as an Error instance
    if (error instanceof Error) {
      throw error;
    }
    throw new Error(String(error));
  }
}