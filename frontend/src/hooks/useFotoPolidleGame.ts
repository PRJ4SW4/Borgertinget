// src/hooks/useFotoPolidleGame.ts
import { useState, useEffect, useCallback } from "react";
import {
  GuessRequestDto,
  GuessResultDto,
  GamemodeTypes,
  PhotoDto as ApiPhotoDto,
  DailyPoliticianDto,
} from "../types/PolidleTypes";
import { fetchPhotoOfTheDay, submitGuess } from "../services/PolidleApiService";

const INITIAL_BLUR_LEVEL = 15; // Start blur-værdi (pixels)
const BLUR_REDUCTION_PER_GUESS = 3; // Hvor meget blur reduceres pr. forkert gæt
const MIN_BLUR_LEVEL = 0;

interface FotoGuessHistoryItemInternal {
  guessedInfo: DailyPoliticianDto;
  isCorrect: boolean;
}

interface UseFotoPolidleGameReturn {
  photoUrl: string | null;
  isLoadingPhoto: boolean;
  photoError: string | null;
  blurLevel: number;
  guessHistory: FotoGuessHistoryItemInternal[];
  isGuessing: boolean;
  guessError: string | null;
  isGameWon: boolean;
  makeFotoGuess: (politicianId: number) => Promise<GuessResultDto | null>;
  resetGame: () => void;
}

export const useFotoPolidleGame = (): UseFotoPolidleGameReturn => {
  const [photoUrl, setPhotoUrl] = useState<string | null>(null);
  const [isLoadingPhoto, setIsLoadingPhoto] = useState<boolean>(true);
  const [photoError, setPhotoError] = useState<string | null>(null);
  const [blurLevel, setBlurLevel] = useState<number>(INITIAL_BLUR_LEVEL);

  const [guessHistory, setGuessHistory] = useState<
    FotoGuessHistoryItemInternal[]
  >([]);
  const [isGuessing, setIsGuessing] = useState<boolean>(false);
  const [guessError, setGuessError] = useState<string | null>(null);
  const [isGameWon, setIsGameWon] = useState<boolean>(false);

  const loadPhoto = useCallback(async () => {
    setIsLoadingPhoto(true);
    setPhotoError(null);
    setPhotoUrl(null); // Ryd tidligere foto
    try {
      const data: ApiPhotoDto = await fetchPhotoOfTheDay();
      setPhotoUrl(data.photoUrl || null); // Sæt til null hvis undefined
      if (!data.photoUrl) {
        setPhotoError("Ingen billed-URL modtaget fra serveren.");
      }
    } catch (error) {
      console.error("Fetch photo error in hook:", error);
      if (error instanceof Error) {
        setPhotoError(error.message);
      } else {
        setPhotoError("Ukendt fejl ved hentning af foto.");
      }
    } finally {
      setIsLoadingPhoto(false);
    }
  }, []);

  useEffect(() => {
    loadPhoto();
  }, [loadPhoto]);

  const makeFotoGuess = useCallback(
    async (politicianId: number): Promise<GuessResultDto | null> => {
      if (politicianId === null || isGameWon) {
        return null;
      }
      setIsGuessing(true);
      setGuessError(null);

      const requestBody: GuessRequestDto = {
        guessedPoliticianId: politicianId,
        gameMode: GamemodeTypes.Foto,
      };

      try {
        const resultData = await submitGuess(requestBody);
        if (resultData.guessedPolitician) {
          const historyItem: FotoGuessHistoryItemInternal = {
            guessedInfo: resultData.guessedPolitician,
            isCorrect: resultData.isCorrectGuess,
          };
          setGuessHistory((prevGuesses) => [...prevGuesses, historyItem]);

          if (resultData.isCorrectGuess) {
            setIsGameWon(true);
            setBlurLevel(MIN_BLUR_LEVEL); // Fjern blur helt ved korrekt gæt
          } else {
            // Reducer blur ved forkert gæt
            setBlurLevel((prevBlur) =>
              Math.max(MIN_BLUR_LEVEL, prevBlur - BLUR_REDUCTION_PER_GUESS)
            );
          }
        } else {
          throw new Error("Manglende politiker detaljer i svar fra backend.");
        }
        return resultData;
      } catch (error) {
        console.error("Guess API error in hook (Foto):", error);
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
  );

  const resetGame = useCallback(() => {
    setIsGameWon(false);
    setGuessHistory([]);
    setGuessError(null);
    setBlurLevel(INITIAL_BLUR_LEVEL); // Nulstil blur
    loadPhoto(); // Hent (potentielt) nyt billede
  }, [loadPhoto]);

  return {
    photoUrl,
    isLoadingPhoto,
    photoError,
    blurLevel,
    guessHistory,
    isGuessing,
    guessError,
    isGameWon,
    makeFotoGuess,
    resetGame,
  };
};
