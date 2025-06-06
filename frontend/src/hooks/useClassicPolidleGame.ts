// src/hooks/useClassicPolidleGame.ts
import { useState, useCallback } from "react";
import {
  GuessRequestDto,
  GuessResultDto,
  GamemodeTypes,
  // DailyPoliticianDto // Ikke direkte brugt af hook'en, men af komponenten der viser historik
} from "../types/PolidleTypes";
import { submitGuess } from "../services/PolidleApiService";

interface UseClassicPolidleGameReturn {
  guessResults: GuessResultDto[];
  isGuessing: boolean;
  guessError: string | null;
  isGameWon: boolean; // <<< NYT
  makeGuess: (politicianId: number) => Promise<GuessResultDto | null>;
  resetGame: () => void; // <<< NYT (erstatter/udvider clearGuessHistory)
}

export const useClassicPolidleGame = (): UseClassicPolidleGameReturn => {
  const [guessResults, setGuessResults] = useState<GuessResultDto[]>([]);
  const [isGuessing, setIsGuessing] = useState<boolean>(false);
  const [guessError, setGuessError] = useState<string | null>(null);
  const [isGameWon, setIsGameWon] = useState<boolean>(false); // <<< NY STATE

  const makeGuess = useCallback(
    async (politicianId: number): Promise<GuessResultDto | null> => {
      if (politicianId === null || isGameWon) {
        // <<< TJEK OGSÅ isGameWon
        console.warn(
          "makeGuess kaldt unødvendigt (spil vundet eller intet ID)."
        );
        return null;
      }

      setIsGuessing(true);
      setGuessError(null);

      const requestBody: GuessRequestDto = {
        guessedPoliticianId: politicianId,
        gameMode: GamemodeTypes.Klassisk,
      };

      try {
        const resultData = await submitGuess(requestBody);
        setGuessResults((prevResults) => [...prevResults, resultData]);

        if (resultData.isCorrectGuess) {
          setIsGameWon(true); // <<< SÆT SPILLET TIL VUNDET
        }
        return resultData;
      } catch (error) {
        console.error("Guess API error in hook (Classic):", error);
        if (error instanceof Error) {
          setGuessError(error.message); // Nu er det sikkert at tilgå error.message
        } else {
          // Håndter andre typer af fejl, eller sæt en generisk besked
          setGuessError("Ukendt fejl under afsendelse af gæt.");
        }
        return null;
      } finally {
        setIsGuessing(false);
      }
    },
    [isGameWon]
  ); // Tilføj isGameWon som dependency

  const resetGame = useCallback(() => {
    setGuessResults([]);
    setGuessError(null);
    setIsGameWon(false); // <<< NULSTIL SPILVUNDET STATUS
    console.log("Classic Mode game reset.");
  }, []);

  return {
    guessResults,
    isGuessing,
    guessError,
    isGameWon, // <<< RETURNER
    makeGuess,
    resetGame, // <<< RETURNER
  };
};
