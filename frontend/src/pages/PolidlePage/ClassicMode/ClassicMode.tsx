// src/pages/Polidle/ClassicMode.tsx
import React from "react";
import GuessList from "../../../components/Polidle/GuessList/GuessList";
import Infobox from "../../../components/Polidle/Infobox/Infobox";
import GameSelector from "../../../components/Polidle/GamemodeSelector/GamemodeSelector";
import Input from "../../../components/Polidle/Input/Input"; // Den generiske Input

import { usePoliticianSearch } from "../../../hooks/usePoliticianSearch";
import { useClassicPolidleGame } from "../../../hooks/useClassicPolidleGame";

import pageStyles from "./ClassicMode.module.css"; // <<< Specifikke styles for denne side
import sharedStyles from "../../../components/Polidle/SharedPolidle.module.css"; // <<< Delte Polidle styles

const ClassicMode: React.FC = () => {
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
    guessError: gameGuessError, // Omdøbt for at undgå navnekonflikt
    makeGuess,
    // isGameWon, resetGame // Kan tilføjes hvis ClassicMode skal have "game won" state
  } = useClassicPolidleGame();

  const handleGuessSubmit = async () => {
    if (selectedPoliticianId === null) return;
    const result = await makeGuess(selectedPoliticianId);
    if (result) {
      clearSelectionAndSearch();
      if (result.isCorrectGuess) {
        setTimeout(
          () => alert("Tillykke, du gættede rigtigt! (Classic Mode)"),
          100
        );
        // Her kan du evt. kalde en resetGame() fra hook'en, hvis spillet skal nulstilles
      }
    }
  };

  return (
    <div
      className={`${pageStyles.pageContainer} ${sharedStyles.gamePageContainer}`}
    >
      {" "}
      {/* Eksempel: kombinerer side-specifik og delt stil */}
      <h1 className={sharedStyles.gameHeader}>Polidle - Klassisk Mode</h1>{" "}
      {/* Bruger delt stil */}
      <GameSelector />
      <p className={sharedStyles.gameInstructions}>Gæt dagens politiker</p>{" "}
      {/* Bruger delt stil */}
      <div className={sharedStyles.searchContainer}>
        {" "}
        {/* Bruger delt stil */}
        <Input
          type="text"
          placeholder="Skriv navn på politiker..."
          value={searchText}
          onChange={handleSearchChange}
          disabled={isGuessing}
          className={sharedStyles.searchInput} // Bruger delt stil for input
          autoComplete="off"
        />
        {searchText && selectedPoliticianId === null && (
          <>
            {isSearching && (
              <div className={sharedStyles.searchLoader}>Søger...</div>
            )}
            {searchError && (
              <div className={sharedStyles.searchError}>
                Fejl: {searchError}
              </div>
            )}
            {!isSearching &&
              !searchError &&
              searchResults.length === 0 &&
              searchText.length > 0 && (
                <div className={sharedStyles.noResults}>
                  Ingen match fundet.
                </div>
              )}
            {!isSearching && !searchError && searchResults.length > 0 && (
              <ul className={sharedStyles.searchResults}>
                {searchResults.map((option) => (
                  <li
                    key={option.id}
                    onClick={() => handleOptionSelect(option)}
                    className={sharedStyles.searchResultItem}
                  >
                    {option.pictureUrl ? (
                      <img
                        src={option.pictureUrl}
                        alt={option.politikerNavn}
                        className={sharedStyles.searchResultImage}
                      />
                    ) : (
                      <div
                        className={sharedStyles.searchResultImagePlaceholder}
                      >
                        ?
                      </div>
                    )}
                    <span className={sharedStyles.searchResultName}>
                      {option.politikerNavn}
                    </span>
                  </li>
                ))}
              </ul>
            )}
          </>
        )}
        <button
          onClick={handleGuessSubmit}
          disabled={isGuessing || selectedPoliticianId === null}
          className={sharedStyles.guessButton} // Bruger delt stil
        >
          {isGuessing ? "Gætter..." : "Gæt"}
        </button>
        {gameGuessError && (
          <div className={sharedStyles.guessError}>Fejl: {gameGuessError}</div>
        )}
      </div>
      <div className={sharedStyles.guessListContainer}>
        {" "}
        {/* Bruger delt stil */}
        <GuessList results={guessResults} />
      </div>
      <div className={sharedStyles.infoboxContainer}>
        {" "}
        {/* Bruger delt stil */}
        <Infobox />
      </div>
    </div>
  );
};

export default ClassicMode;
