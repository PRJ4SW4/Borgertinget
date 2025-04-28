// Fil: src/types/polidleTypes.ts (eller hvor du placerer den)

// Bruges i søgeresultater fra GET /api/polidle/politicians
// Matcher din backend PoliticianSummaryDto
export interface PoliticianOption {
  id: number;
  politikerNavn: string;
  portraet: number[]; // Repræsenterer byte[] fra C#
}

// Bruges som request body til POST /api/polidle/guess
export interface GuessRequestDto {
  guessedPoliticianId: number;
  // Tillad både tal (hvis backend binder til int enum) og streng (hvis backend bruger string enum converter)
  gameMode: number | string;
}

// Bruges i svaret (GuessResultDto) fra POST /api/polidle/guess
export enum FeedbackType {
  Undefined = 0,
  Korrekt = 1,
  Forkert = 2,
  Højere = 3, // Korrekt alder er højere end gættet
  Lavere = 4, // Korrekt alder er lavere end gættet
}

// Bruges inde i GuessResultDto
export interface GuessedPoliticianDetailsDto {
  id: number;
  politikerNavn: string;
  partiNavn: string;
  age: number; // Vigtigt: Hedder 'age' for at matche JSON fra backend
  køn: string;
  uddannelse: string;
  region: string;
  portraet: number[];
}

// Hele svaret fra POST /api/polidle/guess
export interface GuessResultDto {
  isCorrectGuess: boolean;
  // Feedback dictionary: Key er feltnavn (string), Value er FeedbackType (enum/tal)
  feedback: Record<string, FeedbackType>; // Record<string, FeedbackType> er det samme som { [key: string]: FeedbackType }
  guessedPolitician: GuessedPoliticianDetailsDto | null;
}

// Bruges til svaret fra GET /api/polidle/daily/quote
export interface QuoteDto {
  quoteText: string;
  // Tilføj evt. politikerId hvis backend sender det
}

// Bruges til svaret fra GET /api/polidle/daily/photo
export interface PhotoDto {
  portraitBase64: string; // Antager backend sender Base64 for nemheds skyld
  // Tilføj evt. politikerId hvis backend sender det
}

// Type for historik-elementer i CitatMode state (kan blive her eller flyttes)
export interface CitatGuessHistoryItem {
  guessedInfo: GuessedPoliticianDetailsDto;
  isCorrect: boolean;
}

// Tilføj andre typer, f.eks. for ClassicMode state hvis nødvendigt
// export interface ClassicGuessHistoryItem extends GuessResultDto {} // Kunne være en mulighed

// Enum for selve gamemodes - match værdier/navne med din backend C# enum
// Dette er nyttigt til at sende korrekt værdi i GuessRequestDto
export enum GameMode {
  Klassisk = 0, // Eller "Klassisk" hvis du bruger string converter
  Citat = 1, // Eller "Citat"
  Foto = 2, // Eller "Foto"
}
