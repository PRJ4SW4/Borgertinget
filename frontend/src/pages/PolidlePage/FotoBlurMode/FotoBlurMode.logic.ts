// src/pages/Polidle/FotoBlurMode.logic.ts
import { usePoliticianSearch } from "../../../hooks/usePoliticianSearch";
import { useFotoPolidleGame } from "../../../hooks/useFotoPolidleGame";

export const useFotoBlurModePage = () => {
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
  } = useFotoPolidleGame();

  const handleGuessSubmit = async () => {
    if (selectedPoliticianId === null || isGameWon) return;

    const result = await makeFotoGuess(selectedPoliticianId);
    if (result) {
      clearSelectionAndSearch();
      if (result.isCorrectGuess) {
        setTimeout(
          () => alert("Tillykke, du gættede rigtigt! (Foto Mode)"),
          100
        );
      }
    }
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

    photoUrl,
    isLoadingPhoto,
    photoError,
    blurLevel,
    guessHistory,
    isGuessing,
    guessError,
    isGameWon,
    resetGame,

    handleGuessSubmit,
  };
};
