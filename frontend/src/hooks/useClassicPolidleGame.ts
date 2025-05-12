// src/hooks/useClassicPolidleGame.ts
import { useState, useCallback } from "react";
import {
  GuessRequestDto,
  GuessResultDto,
  GamemodeTypes,
} from "../types/PolidleTypes"; // << VIGTIGT: Sørg for korrekt sti
import { submitGuess } from "../services/polidleApiService"; // << VIGTIGT: Sørg for korrekt sti

interface UseClassicPolidleGameReturn {
  guessResults: GuessResultDto[];
  isGuessing: boolean;
  guessError: string | null;
  makeGuess: (politicianId: number) => Promise<GuessResultDto | null>; // Returnerer resultatet for evt. specifik håndtering
  clearGuessHistory: () => void;
}

export const useClassicPolidleGame = (): UseClassicPolidleGameReturn => {
  const [guessResults, setGuessResults] = useState<GuessResultDto[]>([]);
  const [isGuessing, setIsGuessing] = useState<boolean>(false);
  const [guessError, setGuessError] = useState<string | null>(null);

  const makeGuess = useCallback(
    async (politicianId: number): Promise<GuessResultDto | null> => {
      if (politicianId === null) {
        // Dette tjek bør ske før kald af makeGuess, men som en sikkerhedsforanstaltning.
        console.warn("makeGuess kaldt med null politicianId");
        return null;
      }

      setIsGuessing(true);
      setGuessError(null);

      const requestBody: GuessRequestDto = {
        guessedPoliticianId: politicianId,
        gameMode: GamemodeTypes.Klassisk, // Brug enum for klarhed
      };

      try {
        const resultData = await submitGuess(requestBody);
        setGuessResults((prevResults) => [...prevResults, resultData]);
        // Håndter evt. "game won" state her, hvis spillet skal stoppe ved korrekt gæt
        if (resultData.isCorrectGuess) {
          // console.log("Korrekt gæt! Spillet kan afsluttes/nulstilles her.");
          // setTimeout(() => alert("Tillykke, du gættede rigtigt!"), 100); // Eksempel
        }
        return resultData;
      } catch (error: any) {
        console.error("Guess API error in hook:", error);
        setGuessError(error.message || "Ukendt fejl under afsendelse af gæt.");
        return null;
      } finally {
        setIsGuessing(false);
      }
    },
    []
  ); // tom dependencies array, da submitGuess og GameMode.Klassisk ikke ændrer sig

  const clearGuessHistory = useCallback(() => {
    setGuessResults([]);
  }, []);

  return {
    guessResults,
    isGuessing,
    guessError,
    makeGuess,
    clearGuessHistory,
  };
};
