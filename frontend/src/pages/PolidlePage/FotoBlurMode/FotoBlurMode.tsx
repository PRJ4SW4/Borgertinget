// src/pages/Polidle/FotoBlurMode.tsx
import React from "react";
import GameSelector from "../../../components/Polidle/GamemodeSelector/GamemodeSelector";
import Input from "../../../components/Polidle/Input/Input";
import { useFotoBlurModePage } from "./FotoBlurMode.logic";

import pageStyles from "./FotoBlurMode.module.css";
import sharedStyles from "../../../components/Polidle/SharedPolidle.module.css";

const FotoBlurMode: React.FC = () => {
  const {
    searchText,
    searchResults,
    isSearching,
    searchError,
    selectedPoliticianId,
    handleSearchChange,
    handleOptionSelect,

    photoUrl,
    isLoadingPhoto,
    photoError,
    blurLevel,
    guessHistory,
    isGuessing,
    guessError,
    isGameWon,
    handleGuessSubmit,
    resetGame,
  } = useFotoBlurModePage();

  return (
    <div
      className={`${pageStyles.pageContainer} ${sharedStyles.gamePageContainer}`}
    >
      <h1 className={sharedStyles.gameHeader}>Polidle - Foto Mode</h1>
      <GameSelector />

      <div className={pageStyles.photoDisplayContainer}>
        <p className={sharedStyles.gameInstructions}>Hvem er på billedet?</p>
        {isLoadingPhoto && <p>Henter dagens billede...</p>}
        {photoError && (
          <p className={pageStyles.photoError}>Fejl: {photoError}</p>
        )}
        {!isLoadingPhoto && !photoError && photoUrl && (
          <img
            src={photoUrl}
            alt="Sløret politiker portræt"
            className={pageStyles.blurredImage}
            style={{ filter: `blur(${blurLevel}px)` }}
          />
        )}
        {!isLoadingPhoto && !photoError && !photoUrl && (
          <p className={pageStyles.photoError}>
            Kunne ikke finde et billede for i dag.
          </p>
        )}
      </div>

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
              {!isSearching && !searchError && searchResults.length === 0 && (
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
          <p>Godt gået! Du fandt politikeren!</p>
          <button onClick={resetGame} className={sharedStyles.playAgainButton}>
            Spil Igen
          </button>
        </div>
      )}

      <div
        className={`${sharedStyles.fotoGuessHistory} ${pageStyles.fotoModeGuessHistory}`}
      >
        {guessHistory.length > 0 && (
          <h3 className={sharedStyles.historyHeader}>Dine Gæt:</h3>
        )}
        {guessHistory.map((item, index) => (
          <div
            key={index}
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

export default FotoBlurMode;
