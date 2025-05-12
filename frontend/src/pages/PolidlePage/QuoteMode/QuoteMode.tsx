// src/pages/Polidle/CitatMode.tsx
import React from "react";
import GameSelector from "../../../components/Polidle/GamemodeSelector/GamemodeSelector";
import Input from "../../../components/Polidle/Input/Input";

import { usePoliticianSearch } from "../../../hooks/usePoliticianSearch";
import { useCitatPolidleGame } from "../../../hooks/useCitatPolidleGame";
import { DailyPoliticianDto } from "../../../types/PolidleTypes";

import pageStyles from "./CitatMode.module.css"; // <<< Specifikke styles
import sharedStyles from "../../../components/Polidle/SharedPolidle.module.css";

interface CitatGuessHistoryDisplayItem {
  guessedInfo: DailyPoliticianDto;
  isCorrect: boolean;
}

const CitatMode: React.FC = () => {
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
    quote,
    isLoadingQuote,
    quoteError,
    guessHistory,
    isGuessing,
    guessError: gameGuessError,
    isGameWon,
    makeCitatGuess,
    resetGame,
  } = useCitatPolidleGame();

  const handleGuessSubmit = async () => {
    // ... (samme som i ClassicMode, men kalder makeCitatGuess) ...
    if (selectedPoliticianId === null || isGameWon) return;
    const result = await makeCitatGuess(selectedPoliticianId);
    if (result) {
      clearSelectionAndSearch();
      if (result.isCorrectGuess) {
        setTimeout(
          () => alert("Tillykke, du gættede rigtigt! (Citat Mode)"),
          100
        );
      }
    }
  };

  return (
    <div
      className={`${pageStyles.pageContainer} ${sharedStyles.gamePageContainer}`}
    >
      <h1 className={sharedStyles.gameHeader}>Polidle - Citat Mode</h1>
      <GameSelector />

      <div className={pageStyles.quoteDisplayContainer}>
        <p className={sharedStyles.gameInstructions}>Hvem sagde dette citat?</p>
        {isLoadingQuote && <p>Henter dagens citat...</p>}
        {quoteError && (
          <p className={pageStyles.quoteError}>Fejl: {quoteError}</p>
        )}
        {!isLoadingQuote && !quoteError && quote && (
          <blockquote className={pageStyles.quoteText}>"{quote}"</blockquote>
        )}
        {!isLoadingQuote && !quoteError && !quote && (
          <p className={pageStyles.quoteError}>
            Kunne ikke finde et citat for i dag.
          </p>
        )}
      </div>

      {/* Søge- og gætte-sektion (genbruger sharedStyles) */}
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
          {/* ... Søgeresultater (som i ClassicMode, bruger sharedStyles) ... */}
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
            disabled={isGuessing || selectedPoliticianId === null || isGameWon}
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
      {isGameWon && (
        <div className={sharedStyles.gameWonMessage}>
          <p>Godt gået! Du fandt politikeren!</p>
          <button onClick={resetGame} className={sharedStyles.playAgainButton}>
            Spil Igen
          </button>
        </div>
      )}

      {/* Gættehistorik (bruger delte styles fra PolidleStyles.module.css for .citatGuessHistory etc.) */}
      <div className={sharedStyles.citatGuessHistory}>
        {guessHistory.length > 0 && (
          <h3 className={sharedStyles.historyHeader}>Dine Gæt:</h3>
        )}
        {guessHistory.map((item: CitatGuessHistoryDisplayItem, index) => (
          <div
            key={index} // Overvej bedre key
            className={`${sharedStyles.citatGuessItem} ${
              item.isCorrect ? sharedStyles.correct : sharedStyles.incorrect
            }`}
          >
            {item.guessedInfo?.pictureUrl && (
              <img
                src={item.guessedInfo.pictureUrl}
                alt={item.guessedInfo.politikerNavn}
                className={sharedStyles.historyImage}
              />
            )}
            <span className={sharedStyles.historyName}>
              {item.guessedInfo?.politikerNavn ?? "Ukendt"}
            </span>
            <span className={sharedStyles.historyIndicator}>
              {item.isCorrect ? "✓" : "✕"}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
};

export default CitatMode;
