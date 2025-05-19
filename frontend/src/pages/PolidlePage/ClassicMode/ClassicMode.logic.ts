// src/pages/Polidle/ClassicMode.logic.ts
import { usePoliticianSearch } from "../../../hooks/usePoliticianSearch";
import { useClassicPolidleGame } from "../../../hooks/useClassicPolidleGame";

export const useClassicModeLogic = () => {
  const {
    searchText,
    searchResults,
    isSearching,
    searchError,
    selectedPoliticianId,
    handleSearchChange,
    handleOptionSelect,
    clearSelectionAndSearch,
  } = usePoliticianSearch();

  const {
    guessResults,
    isGuessing,
    guessError,
    isGameWon,
    makeGuess,
    resetGame,
  } = useClassicPolidleGame();

  const handleGuessSubmit = async () => {
    if (selectedPoliticianId === null || isGameWon) return;
    const result = await makeGuess(selectedPoliticianId);
    if (result) {
      clearSelectionAndSearch();
    }
  };

  const handlePlayAgain = () => {
    resetGame();
    clearSelectionAndSearch();
  };

  return {
    searchText,
    searchResults,
    isSearching,
    searchError,
    selectedPoliticianId,
    handleSearchChange,
    handleOptionSelect,
    clearSelectionAndSearch,
    guessResults,
    isGuessing,
    guessError,
    isGameWon,
    handleGuessSubmit,
    handlePlayAgain,
  };
};
