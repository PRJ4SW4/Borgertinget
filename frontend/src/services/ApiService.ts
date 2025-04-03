// src/services/apiService.ts
import type { PageSummaryDto, PageDetailDto, AnswerOptionDto, QuestionDto } from '../types/pageTypes'; // Import types
import type { FlashcardCollectionSummaryDto, FlashcardCollectionDetailDto } from '../types/flashcardTypes';

const API_BASE_URL = '/api'; // Adjust if needed

// --- Add Types for Answer Checking ---
interface AnswerCheckRequest {
  questionId: number;
  selectedAnswerOptionId: number;
}

export interface AnswerCheckResponse { // Export if needed elsewhere
  isCorrect: boolean;
  // correctAnswerOptionId?: number; // Optional based on backend DTO
}
// ---

// --- Add Function to Call Backend ---
export const checkAnswer = async (payload: AnswerCheckRequest): Promise<AnswerCheckResponse> => {
  const response = await fetch(`/api/answers/check`, { // Match backend route
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      // Include Authorization header if your endpoint requires login
      // 'Authorization': `Bearer ${localStorage.getItem('jwt')}`
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    // Handle API errors (like the BadRequest from backend)
    const errorText = await response.text(); // Get error details if possible
    console.error("API Error:", response.status, errorText);
    throw new Error(`Failed to check answer (${response.status})`);
  }
  return await response.json() as AnswerCheckResponse;
};

export const fetchPagesStructure = async (): Promise<PageSummaryDto[]> => {
  const response = await fetch(`${API_BASE_URL}/pages`);
  if (!response.ok) {
    // Consider more specific error handling based on status code
    throw new Error(`Failed to fetch page structure: ${response.statusText}`);
  }
  // Explicitly type the response parsing
  return await response.json() as PageSummaryDto[];
};

export const fetchPageDetails = async (id: string | number): Promise<PageDetailDto | null> => {
  // Ensure id is valid before fetching
  if (!id) return null;
  const response = await fetch(`${API_BASE_URL}/pages/${id}`);
  if (response.status === 404) {
     return null; // Page not found
  }
  if (!response.ok) {
    throw new Error(`Failed to fetch page details for ID ${id}: ${response.statusText}`);
  }
  return await response.json() as PageDetailDto;
};

// --- Flashcard Functions ---

// Fetches the list of all flashcard collections for the sidebar
export const fetchFlashcardCollections = async (): Promise<FlashcardCollectionSummaryDto[]> => {
  const response = await fetch(`${API_BASE_URL}/flashcards/collections`);
  if (!response.ok) {
      console.error("API Error:", response.status, await response.text());
      throw new Error('Failed to fetch flashcard collections');
  }
  return await response.json() as FlashcardCollectionSummaryDto[];
};

// Fetches the details (including all cards) for a single collection by its ID
export const fetchFlashcardCollectionDetails = async (collectionId: string | number): Promise<FlashcardCollectionDetailDto | null> => {
  // Don't fetch if ID is missing or invalid
  if (!collectionId || collectionId === 'undefined' || collectionId === 'null') {
      console.warn("fetchFlashcardCollectionDetails called with invalid ID:", collectionId);
      return null;
  }
  const response = await fetch(`${API_BASE_URL}/flashcards/collections/${collectionId}`);

  if (response.status === 404) {
      console.warn(`Flashcard collection not found: ID ${collectionId}`);
      return null; // Return null if collection doesn't exist
  }
  if (!response.ok) {
      console.error("API Error:", response.status, await response.text());
      throw new Error(`Failed to fetch details for flashcard collection ${collectionId}`);
  }
  return await response.json() as FlashcardCollectionDetailDto;
};