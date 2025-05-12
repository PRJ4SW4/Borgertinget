// src/pages/Polidle/FotoBlurMode.tsx
import React from "react";
import GameSelector from "../../../components/Polidle/GamemodeSelector/GamemodeSelector";
import Input from "../../../components/Polidle/Input/Input";

import { usePoliticianSearch } from "../../../hooks/usePoliticianSearch";
import { useFotoPolidleGame } from "../../../hooks/useFotoPolidleGame"; // Antager denne hook findes
import { DailyPoliticianDto } from "../../../types/PolidleTypes";

import pageStyles from "./FotoBlurMode.module.css"; // <<< Specifikke styles
import sharedStyles from "../../../components/Polidle/SharedPolidle.module.css"; // <<< Delte Polidle styles

interface FotoGuessHistoryDisplayItem {
  guessedInfo: DailyPoliticianDto;
  isCorrect: boolean;
}

const FotoBlurMode: React.FC = () => {
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
    guessError: gameGuessError,
    isGameWon,
    makeFotoGuess,
    resetGame,
  } = useFotoPolidleGame(); // Brug den korrekte hook

  const handleGuessSubmit = async () => {
    // ... (samme som i ClassicMode, men kalder makeFotoGuess) ...
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

  return (
    <div
      className={`${pageStyles.pageContainer} ${sharedStyles.gamePageContainer}`}
    >
      <h1 className={sharedStyles.gameHeader}>Polidle - Foto Sløret</h1>
      <GameSelector />

      <div className={pageStyles.photoDisplayContainer}>
        {" "}
        {/* Bruges i stedet for polidleStyles.photoContainer */}
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

      {/* Gættehistorik (genbruger sharedStyles for .citatGuessHistory osv.) */}
      <div
        className={`${sharedStyles.fotoGuessHistory} ${pageStyles.fotoModeGuessHistory}`}
      >
        {" "}
        {/* Kan have både delt og specifik klasse */}
        {guessHistory.length > 0 && (
          <h3 className={sharedStyles.historyHeader}>Dine Gæt:</h3>
        )}
        {guessHistory.map((item: FotoGuessHistoryDisplayItem, index) => (
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
