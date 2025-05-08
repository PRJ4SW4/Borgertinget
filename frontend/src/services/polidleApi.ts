// Fil: src/services/polidleApi.ts
import {
  PoliticianSummaryDto,
  GuessRequestDto,
  GuessResultDto,
  QuoteDto,
  PhotoDto,
} from "../types/polidleTypes"; // Importer typer

// Define your backend base URL - consider moving this to environment variables
const API_BASE_URL = "/api/polidle"; // Use relative path if served from same domain, or full URL

// Helper function to handle fetch responses and errors
async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    let errorMsg = `API Error: ${response.status} ${response.statusText}`;
    try {
      // Try to parse error details from backend response body
      const errorData = await response.json();
      errorMsg = `${errorMsg} - ${
        errorData.message || errorData.title || JSON.stringify(errorData)
      }`;
    } catch (e) {
      // If parsing fails, use the status text
    }
    throw new Error(errorMsg);
  }
  // Handle cases where the response might be empty (e.g., 204 No Content)
  const contentType = response.headers.get("content-type");
  if (contentType && contentType.indexOf("application/json") !== -1) {
    return (await response.json()) as T;
  } else {
    // Return null or an appropriate default value for non-JSON responses if expected
    // Or handle as text if necessary: await response.text();
    return null as T; // Adjust based on expected non-JSON responses
  }
}

/**
 * Fetches politician summaries based on search text.
 * Corresponds to GET /api/polidle/politicians?search={searchText}
 * @param searchText The text to search for.
 * @returns A promise resolving to an array of PoliticianSummaryDto.
 */
export async function searchPoliticians(
  searchText: string
): Promise<PoliticianSummaryDto[]> {
  if (!searchText.trim()) {
    return []; // Return empty array if search text is empty
  }
  const encodedSearch = encodeURIComponent(searchText);
  // Adjust endpoint if your backend controller uses a different route, e.g., /api/Polidle/all
  const apiUrl = `${API_BASE_URL}/all?search=${encodedSearch}`; // Corrected endpoint based on likely controller structure

  // Add Authorization header if needed
  const token = localStorage.getItem("jwt");
  const headers: HeadersInit = {
    Accept: "application/json",
    // ...(token ? { 'Authorization': `Bearer ${token}` } : {}) // Uncomment if auth is needed
  };

  const response = await fetch(apiUrl, { headers });
  return handleResponse<PoliticianSummaryDto[]>(response);
}

/**
 * Submits a guess to the backend.
 * Corresponds to POST /api/polidle/guess
 * @param guessData The guess request data.
 * @returns A promise resolving to a GuessResultDto.
 */
export async function submitGuess(
  guessData: GuessRequestDto
): Promise<GuessResultDto> {
  const apiUrl = `${API_BASE_URL}/guess`;

  const token = localStorage.getItem("jwt");
  const headers: HeadersInit = {
    "Content-Type": "application/json",
    Accept: "application/json",
    // ...(token ? { 'Authorization': `Bearer ${token}` } : {}) // Uncomment if auth is needed
  };

  const response = await fetch(apiUrl, {
    method: "POST",
    headers: headers,
    body: JSON.stringify(guessData),
  });

  return handleResponse<GuessResultDto>(response);
}

/**
 * Fetches the quote of the day.
 * Corresponds to GET /api/polidle/quote
 * @returns A promise resolving to a QuoteDto.
 */
export async function getQuoteOfTheDay(): Promise<QuoteDto> {
  const apiUrl = `${API_BASE_URL}/quote`; // Endpoint for getting the quote

  const token = localStorage.getItem("jwt");
  const headers: HeadersInit = {
    Accept: "application/json",
    // ...(token ? { 'Authorization': `Bearer ${token}` } : {}) // Uncomment if auth is needed
  };

  const response = await fetch(apiUrl, { headers });
  return handleResponse<QuoteDto>(response);
}

/**
 * Fetches the photo of the day.
 * Corresponds to GET /api/polidle/photo
 * @returns A promise resolving to a PhotoDto.
 */
export async function getPhotoOfTheDay(): Promise<PhotoDto> {
  const apiUrl = `${API_BASE_URL}/photo`; // Endpoint for getting the photo

  const token = localStorage.getItem("jwt");
  const headers: HeadersInit = {
    Accept: "application/json",
    // ...(token ? { 'Authorization': `Bearer ${token}` } : {})
  };

  const response = await fetch(apiUrl, { headers });
  // Ensure PhotoDto is handled correctly
  const result = await handleResponse<PhotoDto | null>(response);
  if (result === null) {
    throw new Error("Received unexpected empty response when fetching photo.");
  }
  return result;
}
