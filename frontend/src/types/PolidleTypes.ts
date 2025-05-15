/**
 * Enum for de forskellige spil-modes.
 * Værdierne (0, 1, 2) skal matche backend's C# GamemodeTypes enum.
 */
export enum GamemodeTypes {
  Klassisk = 0,
  Citat = 1,
  Foto = 2,
}

/**
 * Enum for feedback-typer på et gæt for specifikke felter.
 * Værdierne (0-4) skal matche backend's C# FeedbackType enum.
 */
export enum FeedbackType {
  Undefined = 0,
  Korrekt = 1,
  Forkert = 2,
  Højere = 3, // Korrekt værdi er højere end gættet (bruges f.eks. til Alder)
  Lavere = 4, // Korrekt værdi er lavere end gættet (bruges f.eks. til Alder)
}

/**
 * DTO for at vise en politiker i en søgeliste (f.eks. autocomplete).
 * Bruges når brugeren skal vælge en politiker at gætte på.
 * Svarer til backend's DailyPoliticianDto (som tidligere blev kaldt PoliticianSummaryDto på backend).
 */
export interface SearchListDto {
  id: number;
  politikerNavn: string;
  pictureUrl?: string;
}

/**
 * DTO med detaljeret information om en politiker.
 * Bruges til:
 * 1. At vise information om den gættede politiker (i GuessResultDto).
 * 2. At hente dagens politiker for Classic Mode.
 * Svarer til backend's DailyPoliticianDto.
 */
export interface DailyPoliticianDto {
  id: number;
  politikerNavn: string;
  pictureUrl?: string;
  køn?: string;
  parti?: string;
  age: number;
  region?: string;
  uddannelse?: string;
}

/**
 * DTO for dagens citat i Citat-gamemode.
 * Svarer til backend's QuoteDto.
 */
export interface QuoteDto {
  quoteText: string;
}

/**
 * DTO for dagens foto i Foto-gamemode.
 * Svarer til backend's PhotoDto.
 */
export interface PhotoDto {
  photoUrl?: string;
}

/**
 * DTO der sendes FRA frontend TIL backend, når en bruger laver et gæt.
 * Svarer til backend's GuessRequestDto.
 */
export interface GuessRequestDto {
  guessedPoliticianId: number;
  gameMode: GamemodeTypes;
}

/**
 * DTO der modtages FRA backend TIL frontend med resultatet af et gæt.
 * Svarer til backend's GuessResultDto (forenklet til ubegrænsede gæt).
 */
export interface GuessResultDto {
  isCorrectGuess: boolean;
  feedback: Record<string, FeedbackType>;
  /**
   * Detaljer om den politiker, der blev gættet på.
   * Hvis isCorrectGuess er true, er dette den korrekte politiker.
   * Skal være DailyPoliticianDto for at give fuld info til feedback-visning.
   */
  guessedPolitician?: DailyPoliticianDto; // <<< OPDATERET til at bruge det nye DTO-navn
}

// ------ UI-Specifikke Typer (kan udvides) ------

/**
 * Bruges til at vise gæt-historik for Classic Mode.
 * Indeholder den fulde GuessResultDto for hvert gæt.
 */
export type ClassicGuessHistoryItem = GuessResultDto;

/**
 * Bruges til at vise gæt-historik for Citat/Foto Mode,
 * hvor vi måske kun viser navn, billede og om det var korrekt.
 */
export interface SimpleGuessHistoryItem {
  guessedPoliticianName: string;
  guessedPoliticianPictureUrl?: string;
  isCorrect: boolean;
  // guessedPoliticianId: number; // Kan tilføjes hvis nødvendigt
}
