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
    guessError: gameGuessError,
    makeGuess,
    isGameWon, // <<< HENT FRA HOOK
    resetGame, // <<< HENT FRA HOOK
  } = useClassicPolidleGame();

  const handleGuessSubmit = async () => {
    if (selectedPoliticianId === null || isGameWon) return; // Tjek isGameWon her også
    const result = await makeGuess(selectedPoliticianId);
    if (result) {
      clearSelectionAndSearch();
      // Logik for "game won" alert håndteres nu af isGameWon state og JSX nedenfor
      // if (result.isCorrectGuess) {
      //   setTimeout(() => alert("Tillykke, du gættede rigtigt! (Classic Mode)"), 100);
      // }
    }
  };

  const handlePlayAgain = () => {
    resetGame();
    clearSelectionAndSearch(); // Ryd også søgefeltet for den nye runde
  };

  return (
    <div
      className={`${pageStyles.pageContainer} ${sharedStyles.gamePageContainer}`}
    >
      <h1 className={sharedStyles.gameHeader}>Polidle - Klassisk Mode</h1>
      <GameSelector />
      <p className={sharedStyles.gameInstructions}>Gæt dagens politiker</p>

      {/* --- Viser kun søgning/gæt sektion HVIS spillet IKKE er vundet --- */}
      {!isGameWon && (
        <div className={sharedStyles.searchContainer}>
          <Input
            type="text"
            placeholder="Skriv navn på politiker..."
            value={searchText}
            onChange={handleSearchChange}
            disabled={isGuessing || isGameWon} // <<< Deaktiver hvis spillet er vundet
            className={sharedStyles.searchInput}
            autoComplete="off"
          />
          {/* Søgeresultater (som før) */}
          {searchText &&
            selectedPoliticianId === null &&
            !isGameWon && ( // <<< Tjek isGameWon
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
                            className={
                              sharedStyles.searchResultImagePlaceholder
                            }
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
            disabled={isGuessing || selectedPoliticianId === null || isGameWon} // <<< Deaktiver hvis spillet er vundet
            className={sharedStyles.guessButton}
          >
            {isGuessing ? "Gætter..." : "Gæt"}
          </button>
          {gameGuessError && (
            <div className={sharedStyles.guessError}>
              Fejl: {gameGuessError}
            </div>
          )}
        </div>
      )}

      {/* --- Viser "Game Won" besked og "Spil Igen" knap --- */}
      {isGameWon && (
        <div className={sharedStyles.gameWonMessage}>
          {" "}
          {/* Brug den delte stilklasse */}
          <p>Godt gået! Du fandt den rigtige politiker!</p>
          <button
            onClick={handlePlayAgain}
            className={sharedStyles.playAgainButton}
          >
            {" "}
            {/* Brug den delte stilklasse */}
            Spil Igen (Klassisk)
          </button>
        </div>
      )}

      {/* --- Gætte-Liste --- */}
      <div className={sharedStyles.guessListContainer}>
        <GuessList results={guessResults} />
      </div>

      {/* --- Infobox --- */}
      <div className={sharedStyles.infoboxContainer}>
        <Infobox />
      </div>
    </div>
  );
};

export default ClassicMode;
