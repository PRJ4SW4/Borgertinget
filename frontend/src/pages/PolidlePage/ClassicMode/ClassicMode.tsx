// src/pages/Polidle/ClassicMode.tsx
import React from "react";
import GuessList from "../../../components/Polidle/GuessList/GuessList";
import Infobox from "../../../components/Polidle/Infobox/Infobox";
import GameSelector from "../../../components/Polidle/GamemodeSelector/GamemodeSelector";
import Input from "../../../components/Polidle/Input/Input";

import pageStyles from "./ClassicMode.module.css";
import sharedStyles from "../../../components/Polidle/SharedPolidle.module.css";

import { useClassicModeLogic } from "./ClassicMode.logic";

const ClassicMode: React.FC = () => {
  const {
    searchText,
    searchResults,
    isSearching,
    searchError,
    selectedPoliticianId,
    handleSearchChange,
    handleOptionSelect,
    guessResults,
    isGuessing,
    guessError,
    isGameWon,
    handleGuessSubmit,
    handlePlayAgain,
  } = useClassicModeLogic();

  return (
    <div
      className={`${pageStyles.pageContainer} ${sharedStyles.gamePageContainer}`}
    >
      <h1 className={sharedStyles.gameHeader}>Polidle - Klassisk Mode</h1>
      <GameSelector />
      <p className={sharedStyles.gameInstructions}>Gæt dagens politiker</p>

      {!isGameWon && (
        <div className={sharedStyles.searchContainer}>
          <Input
            type="text"
            placeholder="Skriv navn på politiker..."
            value={searchText}
            onChange={handleSearchChange}
            disabled={isGuessing || isGameWon}
            className={sharedStyles.searchInput}
            autoComplete="off"
          />
          {searchText && selectedPoliticianId === null && !isGameWon && (
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
            disabled={isGuessing || selectedPoliticianId === null || isGameWon}
            className={sharedStyles.guessButton}
          >
            {isGuessing ? "Gætter..." : "Gæt"}
          </button>
          {guessError && (
            <div className={sharedStyles.guessError}>Fejl: {guessError}</div>
          )}
        </div>
      )}

      {isGameWon && (
        <div className={sharedStyles.gameWonMessage}>
          <p>Godt gået! Du fandt den rigtige politiker!</p>
          <button
            onClick={handlePlayAgain}
            className={sharedStyles.playAgainButton}
          >
            Spil Igen (Klassisk)
          </button>
        </div>
      )}

      <div className={sharedStyles.guessListContainer}>
        <GuessList results={guessResults} />
      </div>

      <div className={sharedStyles.infoboxContainer}>
        <Infobox />
      </div>
    </div>
  );
};

export default ClassicMode;
