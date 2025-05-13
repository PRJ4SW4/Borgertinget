// src/hooks/useCitatPolidleGame.ts
import { useState, useEffect, useCallback } from "react";
import {
  GuessRequestDto,
  GuessResultDto,
  GamemodeTypes,
  QuoteDto as ApiQuoteDto,
  DailyPoliticianDto, // Til historik
} from "../types/PolidleTypes";
import { fetchQuoteOfTheDay, submitGuess } from "../services/PolidleApiService";

// Definer typen for historik-items her, da den er specifik for denne hook/gamemode
interface CitatGuessHistoryItemInternal {
  guessedInfo: DailyPoliticianDto;
  isCorrect: boolean;
}

interface UseCitatPolidleGameReturn {
  quote: string | null;
  isLoadingQuote: boolean;
  quoteError: string | null;
  guessHistory: CitatGuessHistoryItemInternal[];
  isGuessing: boolean;
  guessError: string | null;
  isGameWon: boolean;
  makeCitatGuess: (politicianId: number) => Promise<GuessResultDto | null>;
  resetGame: () => void; // Til at starte et nyt spil
}

export const useCitatPolidleGame = (): UseCitatPolidleGameReturn => {
  const [quote, setQuote] = useState<string | null>(null);
  const [isLoadingQuote, setIsLoadingQuote] = useState<boolean>(true);
  const [quoteError, setQuoteError] = useState<string | null>(null);

  const [guessHistory, setGuessHistory] = useState<
    CitatGuessHistoryItemInternal[]
  >([]);
  const [isGuessing, setIsGuessing] = useState<boolean>(false);
  const [guessError, setGuessError] = useState<string | null>(null);
  const [isGameWon, setIsGameWon] = useState<boolean>(false);

  const loadQuote = useCallback(async () => {
    setIsLoadingQuote(true);
    setQuoteError(null);
    try {
      const data: ApiQuoteDto = await fetchQuoteOfTheDay();
      setQuote(data.quoteText);
    } catch (error) {
      console.error("Fetch quote error in hook:", error);
      if (error instanceof Error) {
        setQuoteError(error.message);
      } else {
        setQuoteError("Ukendt fejl ved hentning af citat.");
      }
    } finally {
      setIsLoadingQuote(false);
    }
  }, []);

  useEffect(() => {
    loadQuote();
  }, [loadQuote]); // Kør når komponenten mounter og hvis loadQuote ændrer sig (bør ikke ske)

  const makeCitatGuess = useCallback(
    async (politicianId: number): Promise<GuessResultDto | null> => {
      if (politicianId === null || isGameWon) {
        console.warn("makeCitatGuess kaldt unødvendigt eller med null ID.");
        return null;
      }

      setIsGuessing(true);
      setGuessError(null);

      const requestBody: GuessRequestDto = {
        guessedPoliticianId: politicianId,
        gameMode: GamemodeTypes.Citat,
      };

      try {
        const resultData = await submitGuess(requestBody);
        if (resultData.guessedPolitician) {
          // Tjek at vi fik data tilbage
          const historyItem: CitatGuessHistoryItemInternal = {
            guessedInfo: resultData.guessedPolitician, // Dette er DailyPoliticianDto
            isCorrect: resultData.isCorrectGuess,
          };
          setGuessHistory((prevGuesses) => [...prevGuesses, historyItem]);

          if (resultData.isCorrectGuess) {
            setIsGameWon(true);
          }
        } else {
          // Dette bør håndteres af fejl i submitGuess, men som en ekstra sikkerhed
          throw new Error("Manglende politiker detaljer i svar fra backend.");
        }
        return resultData;
      } catch (error) {
        console.error("Guess API error in hook (Citat):", error);
        if (error instanceof Error) {
          setGuessError(error.message);
        } else {
          setGuessError("Ukendt fejl under afsendelse af gæt.");
        }
        return null;
      } finally {
        setIsGuessing(false);
      }
    },
    [isGameWon]
  ); // Afhængig af isGameWon for at stoppe gæt

  const resetGame = useCallback(() => {
    setIsGameWon(false);
    setGuessHistory([]);
    setGuessError(null);
    // Genhent citat for en ny runde
    loadQuote();
  }, [loadQuote]);

  return {
    quote,
    isLoadingQuote,
    quoteError,
    guessHistory,
    isGuessing,
    guessError,
    isGameWon,
    makeCitatGuess,
    resetGame,
  };
};
