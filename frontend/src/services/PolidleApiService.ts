// src/services/PolidleApi.ts
import {
  SearchListDto, // Tidligere DailyPoliticianDto (summary)
  DailyPoliticianDto, // Tidligere PoliticianDetailsDto (details)
  QuoteDto,
  PhotoDto,
  GuessRequestDto,
  GuessResultDto,
  // GameMode, // GameMode enum importeres hvor den bruges, f.eks. i komponenter
} from "../types/PolidleTypes"; // Sørg for korrekt sti til din types fil

// Definer din base API URL her.
// Hvis din frontend serveres fra samme domæne som backend, kan du bruge en relativ sti.
// Ellers skal du bruge den fulde URL til din backend.
const API_BASE_URL = "/api/polidle"; // Base for Polidle controlleren

//REGION: Helper - Api kald og fejl
/**
 * Helper funktion til at håndtere API kald og fejl.
 * Kan udvides til at håndtere tokens, retry-logik osv.
 */
async function handleApiResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    // Forsøg at parse en fejlbesked fra backend, hvis den findes
    let errorMessage = `API fejl: ${response.status} ${response.statusText}`;
    try {
      const errorData = await response.json();
      // Tjek for almindelige fejl DTO strukturer fra ASP.NET Core
      if (errorData && typeof errorData === "object") {
        if ("title" in errorData && typeof errorData.title === "string") {
          errorMessage = errorData.title;
        } else if (
          "message" in errorData &&
          typeof errorData.message === "string"
        ) {
          errorMessage = errorData.message;
        } else if (
          "error" in errorData &&
          typeof errorData.error === "string"
        ) {
          errorMessage = errorData.error;
        }
        // Hvis der er en 'errors' dictionary (typisk fra model validering)
        if (
          "errors" in errorData &&
          typeof errorData.errors === "object" &&
          errorData.errors !== null
        ) {
          const validationErrors = Object.values(errorData.errors)
            .flat()
            .join(", ");
          if (validationErrors) {
            errorMessage += `: ${validationErrors}`;
          }
        }
      }
    } catch (e) {
      // Ignorer parse fejl her, brug den oprindelige status tekst
      console.warn("Kunne ikke parse fejl-body fra API:", e);
    }
    console.error(
      "API Kald Fejlede:",
      errorMessage,
      "Original response:",
      response
    );
    throw new Error(errorMessage);
  }
  // Hvis response er ok, men måske tom (f.eks. 204 No Content)
  const contentType = response.headers.get("content-type");
  if (contentType && contentType.indexOf("application/json") !== -1) {
    return response.json() as Promise<T>;
  } else {
    // Returner null eller en tom default T, hvis der ikke er JSON content
    // Dette afhænger af, hvordan din API opfører sig ved f.eks. 204.
    // For nu antager vi, at endpoints der skal returnere data, altid gør det med JSON.
    return null as T; // Eller en mere specifik håndtering
  }
}

//REGION: Search List
/**
 * Henter en liste af politikere (summaries) til brug i gætte-input,
 * eventuelt filtreret på en søgestreng.
 * @param search Valgfri søgestreng.
 * @returns Et Promise der resolver til en liste af SearchListDto.
 */
export const fetchPoliticiansForSearch = async (
  search?: string
): Promise<SearchListDto[]> => {
  let url = `${API_BASE_URL}/politicians`;
  if (search && search.trim() !== "") {
    url += `?search=${encodeURIComponent(search.trim())}`;
  }
  _logger.LogDebug("Fetching politicians from URL: {url}", url); // Tilføjet for debugging
  const response = await fetch(url);
  return handleApiResponse<SearchListDto[]>(response);
};

//REGION: Dagens Citat
/**
 * Henter dagens citat for Citat-gamemode.
 * @returns Et Promise der resolver til en QuoteDto.
 */
export const fetchQuoteOfTheDay = async (): Promise<QuoteDto> => {
  const url = `${API_BASE_URL}/quote/today`;
  _logger.LogDebug("Fetching quote of the day from URL: {url}", url);
  const response = await fetch(url);
  return handleApiResponse<QuoteDto>(response);
};

//REGION: Dagens Foto
/**
 * Henter URL'en til dagens billede for Foto-gamemode.
 * @returns Et Promise der resolver til en PhotoDto.
 */
export const fetchPhotoOfTheDay = async (): Promise<PhotoDto> => {
  const url = `${API_BASE_URL}/photo/today`;
  _logger.LogDebug("Fetching photo of the day from URL: {url}", url);
  const response = await fetch(url);
  return handleApiResponse<PhotoDto>(response);
};

//REGION: Classic details
/**
 * Henter detaljerne for dagens politiker til Classic-gamemode.
 * @returns Et Promise der resolver til en DailyPoliticianDto (detaljeret).
 */
export const fetchClassicDetailsOfTheDay =
  async (): Promise<DailyPoliticianDto> => {
    const url = `${API_BASE_URL}/classic/today`;
    _logger.LogDebug(
      "Fetching classic details of the day from URL: {url}",
      url
    );
    const response = await fetch(url);
    return handleApiResponse<DailyPoliticianDto>(response);
  };

//REGION: Guess
/**
 * Indsender et gæt til backend.
 * @param guessData Data for gættet (guessedPoliticianId og gameMode).
 * @returns Et Promise der resolver til en GuessResultDto med feedback.
 */
export const submitGuess = async (
  guessData: GuessRequestDto
): Promise<GuessResultDto> => {
  const url = `${API_BASE_URL}/guess`;
  _logger.LogDebug(
    "Submitting guess to URL: {url} with data: {@guessData}",
    url,
    guessData
  );
  const response = await fetch(url, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      // Tilføj Authorization header hvis/når du har brugerlogin for spillet:
      // 'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`,
    },
    body: JSON.stringify(guessData),
  });
  return handleApiResponse<GuessResultDto>(response);
};

// --- Overvejelser til fremtiden (ikke implementeret her): ---
// - Håndtering af JWT tokens for authentication (gemme/sende token).
// - En mere global error handler eller interceptor hvis du bruger f.eks. Axios.
// - Cache-strategier for data der ikke ændrer sig ofte.

// Dummy logger for nu, da vi ikke har en rigtig logger i frontend på samme måde som backend
// Du kan erstatte dette med `console.log`, `console.error`, etc. eller et logging bibliotek
const _logger = {
  LogDebug: (message: string, ...args: any[]) =>
    console.debug(`[DEBUG] ${message}`, ...args),
  LogInformation: (message: string, ...args: any[]) =>
    console.info(`[INFO] ${message}`, ...args),
  LogWarning: (message: string, ...args: any[]) =>
    console.warn(`[WARN] ${message}`, ...args),
  LogError: (error: Error, message: string, ...args: any[]) =>
    console.error(`[ERROR] ${message}`, error, ...args),
};
