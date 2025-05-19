// src/services/PolidleApi.ts
import {
	DailyPoliticianDto,
	GuessRequestDto,
	GuessResultDto,
	PhotoDto,
	QuoteDto,
	SearchListDto,
} from "../types/PolidleTypes"; // Sørg for korrekt sti til din types fil

const API_BASE_URL = "/api/polidle"; // Base for Polidle controlleren

// REGION: Helper - Api kald og fejl
/**
 * Helper funktion til at håndtere API kald og fejl.
 */
async function handleApiResponse<T>( response: Response ): Promise<T>
{
	if ( !response.ok )
	{
		let errorMessage = `API fejl: ${response.status} ${response.statusText}`;
		try
		{
			const errorData = await response.json();
			if ( errorData && typeof errorData === "object" )
			{
				if ( "title" in errorData && typeof errorData.title === "string" )
				{
					errorMessage = errorData.title;
				}
				else if ( "message" in errorData && typeof errorData.message === "string" )
				{
					errorMessage = errorData.message;
				}
				else if ( "error" in errorData && typeof errorData.error === "string" )
				{
					errorMessage = errorData.error;
				}
				if ( "errors" in errorData && typeof errorData.errors === "object" && errorData.errors !== null )
				{
					const validationErrors = Object.values( errorData.errors ).flat().join( ", " );
					if ( validationErrors )
					{
						errorMessage += `: ${validationErrors}`;
					}
				}
			}
		}
		catch ( e )
		{
			console.warn( "Kunne ikke parse fejl-body fra API:", e );
		}
		console.error( "API Kald Fejlede:", errorMessage, "Original response:", response );
		throw new Error( errorMessage );
	}
	const contentType = response.headers.get( "content-type" );
	if ( contentType && contentType.indexOf( "application/json" ) !== -1 )
	{
		return response.json() as Promise<T>;
	}
	else
	{
		return null as T; // Eller en mere specifik håndtering for ikke-JSON svar
	}
}

// REGION: Search List
/**
 * Henter en liste af politikere (summaries) til brug i gætte-input,
 * eventuelt filtreret på en søgestreng.
 */
export const fetchPoliticiansForSearch = async( search?: string ): Promise<SearchListDto[]> => {
	let url = `${API_BASE_URL}/politicians`;
	if ( search && search.trim() !== "" )
	{
		url += `?search=${encodeURIComponent( search.trim() )}`;
	}
	// Bruger _logger defineret i bunden af filen
	_logger.LogDebug( "Fetching politicians from URL: {url}", url );
	const response = await fetch( url, {
		method: "GET",
		headers: {
			"Content-Type": "application/json",
			authorization: `Bearer ${localStorage.getItem( "jwtToken" )}`,
		},
	} );
	return handleApiResponse<SearchListDto[]>( response );
};

// REGION: Dagens Citat
/**
 * Henter dagens citat for Citat-gamemode.
 */
export const fetchQuoteOfTheDay = async(): Promise<QuoteDto> => {
	const url = `${API_BASE_URL}/quote/today`;
	_logger.LogDebug( "Fetching quote of the day from URL: {url}", url );
	const response = await fetch( url, {
		method: "GET",
		headers: {
			"Content-Type": "application/json",
			authorization: `Bearer ${localStorage.getItem( "jwtToken" )}`,
		},
	} );
	return handleApiResponse<QuoteDto>( response );
};

// REGION: Dagens Foto
/**
 * Henter URL'en til dagens billede for Foto-gamemode.
 */
export const fetchPhotoOfTheDay = async(): Promise<PhotoDto> => {
	const url = `${API_BASE_URL}/photo/today`;
	_logger.LogDebug( "Fetching photo of the day from URL: {url}", url );
	const response = await fetch( url, {
		method: "GET",
		headers: {
			"Content-Type": "application/json",
			authorization: `Bearer ${localStorage.getItem( "jwtToken" )}`,
		},
	} );
	return handleApiResponse<PhotoDto>( response );
};

// REGION: Classic details
/**
 * Henter detaljerne for dagens politiker til Classic-gamemode.
 */
export const fetchClassicDetailsOfTheDay = async(): Promise<DailyPoliticianDto> => {
	const url = `${API_BASE_URL}/classic/today`;
	_logger.LogDebug( "Fetching classic details of the day from URL: {url}", url );
	const response = await fetch( url, {
		method: "GET",
		headers: {
			"Content-Type": "application/json",
			authorization: `Bearer ${localStorage.getItem( "jwtToken" )}`,
		},
	} );
	return handleApiResponse<DailyPoliticianDto>( response );
};

// REGION: Guess
/**
 * Indsender et gæt til backend.
 */
export const submitGuess = async( guessData: GuessRequestDto ): Promise<GuessResultDto> => {
	const url = `${API_BASE_URL}/guess`;
	_logger.LogDebug( "Submitting guess to URL: {url} with data: {@guessData}", url, guessData );
	const response = await fetch( url, {
		method: "POST",
		headers: {
			"Content-Type": "application/json",
			Authorization: `Bearer ${localStorage.getItem( 'jwtToken' )}`,
		},
		body: JSON.stringify( guessData ),
	} );
	return handleApiResponse<GuessResultDto>( response );
};

const _logger = {
	LogDebug: (
		message: string,
		...args: unknown[] // <<< ÆNDRET FRA any[] til unknown[]
		) => console.debug( `[DEBUG] ${message}`, ...args ),
	LogInformation: (
		message: string,
		...args: unknown[] // <<< ÆNDRET FRA any[] til unknown[]
		) => console.info( `[INFO] ${message}`, ...args ),
	LogWarning: (
		message: string,
		...args: unknown[] // <<< ÆNDRET FRA any[] til unknown[]
		) => console.warn( `[WARN] ${message}`, ...args ),
	LogError: (
		error: Error, message: string,
		...args: unknown[] // <<< ÆNDRET FRA any[] til unknown[]
		) => console.error( `[ERROR] ${message}`, error, ...args ),
};
