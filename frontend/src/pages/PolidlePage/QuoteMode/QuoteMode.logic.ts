// src/pages/Polidle/QuoteMode.logic.ts
import { useCallback } from "react";
import { usePoliticianSearch } from "../../../hooks/usePoliticianSearch";
import { useCitatPolidleGame } from "../../../hooks/useCitatPolidleGame";

export const useQuoteMode = () => {
  const {
    searchText,
    searchResults,
    isSearching,
    searchError,
    selectedPoliticianId, // Denne er nu tilgængelig for komponenten
    handleSearchChange,
    handleOptionSelect,
    clearSelectionAndSearch,
    setSearchText, // Giver fleksibilitet, hvis direkte manipulation af searchText er nødvendig
  } = usePoliticianSearch();

  const {
    quote,
    isLoadingQuote,
    quoteError,
    guessHistory, // Forventes at være en liste af gæt-objekter
    isGuessing,
    guessError: gameGuessError, // Fejl fra selve gætte-handlingen
    isGameWon,
    makeCitatGuess,
    resetGame: resetCitatGame, // Omdøbt for at undgå navnekonflikt, hvis der var en anden resetGame
  } = useCitatPolidleGame();

  const handleGuessSubmit = useCallback(async () => {
    if (selectedPoliticianId === null || isGameWon) {
      // Undgå at gætte hvis ingen politiker er valgt, eller spillet allerede er vundet.
      return;
    }
    const result = await makeCitatGuess(selectedPoliticianId);
    if (result) {
      clearSelectionAndSearch(); // Ryd søgefelt og valg efter gæt
      if (result.isCorrectGuess) {
        // Selve "Tillykke"-beskeden håndteres nu bedst i komponenten baseret på isGameWon.
        // Den tidligere setTimeout med alert() er fjernet herfra for bedre separation.
        // Komponenten kan vise en succesbesked, når isGameWon bliver true.
      }
    }
  }, [
    selectedPoliticianId,
    isGameWon,
    makeCitatGuess,
    clearSelectionAndSearch,
  ]);

  const resetGame = useCallback(() => {
    resetCitatGame();
    clearSelectionAndSearch(); // Ryd også søgefelt og valg, når spillet nulstilles
  }, [resetCitatGame, clearSelectionAndSearch]);

  return {
    // Fra usePoliticianSearch
    searchText,
    searchResults,
    isSearching,
    searchError,
    selectedPoliticianId, // Nødvendig for at deaktivere gætte-knappen i komponenten
    handleSearchChange,
    handleOptionSelect,
    setSearchText,

    // Fra useCitatPolidleGame
    quote,
    isLoadingQuote,
    quoteError,
    guessHistory,
    isGuessing,
    gameGuessError,
    isGameWon,

    // Kombinerede/egne funktioner
    handleGuessSubmit,
    resetGame,
  };
};
