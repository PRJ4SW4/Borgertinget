// src/pages/Polidle/FotoBlurMode.tsx
import React from "react";
import GameSelector from "../../components/Polidle/GamemodeSelector/GamemodeSelector";
import Input from "../../components/Polidle/Input/Input";

// Importer hooks
import { usePoliticianSearch } from "../../hooks/usePoliticianSearch";
import { useFotoPolidleGame } from "../../hooks/useFotoPolidleGame";

// Importer typer (kun nødvendig for display-delen af historik, hvis forskellig fra hook)
import { DailyPoliticianDto } from "../../types/PolidleTypes";

// Importer styles
import styles from "./Polidle.module.css";
import polidleStyles from "../../components/Polidle/PolidleStyles.module.css";

// Type for historik-visning (kan være den samme som hook'ens interne)
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
  } = useFotoPolidleGame();

  const handleGuessSubmit = async () => {
    if (selectedPoliticianId === null || isGameWon) return;

    const result = await makeFotoGuess(selectedPoliticianId);
    if (result) {
      clearSelectionAndSearch();
      if (result.isCorrectGuess) {
        setTimeout(() => alert("Tillykke, du gættede rigtigt!"), 100);
      }
    }
  };

  // --- JSX Rendering ---
  return (
    <div className={styles.container}>
      <h1 className={styles.heading}>Polidle - Foto Mode</h1>
      <GameSelector />

      {/* --- Vis Foto --- */}
      <div className={polidleStyles.photoContainer}>
        {" "}
        {/* Opret evt. denne klasse for styling af container */}
        <p className={styles.paragraph}>Hvem er på billedet?</p>
        {isLoadingPhoto && <p>Henter dagens billede...</p>}
        {photoError && (
          <p className={polidleStyles.errorText}>Fejl: {photoError}</p>
        )}
        {!isLoadingPhoto && !photoError && photoUrl && (
          <img
            src={photoUrl} // <<< BRUGER photoUrl
            alt="Sløret politiker portræt"
            className={polidleStyles.blurredImage} // Du skal style denne for blur
            style={{ filter: `blur(${blurLevel}px)` }} // Anvend blur-effekt
          />
        )}
        {!isLoadingPhoto && !photoError && !photoUrl && (
          <p style={{ color: "orange" }}>
            Kunne ikke finde et billede for i dag. Prøv at genindlæse siden.
          </p>
        )}
      </div>
      {/* --------------- */}

      {/* --- Politiker Søgning/Valg --- */}
      {!isGameWon && (
        <div className={polidleStyles.searchContainer}>
          <Input
            type="text"
            placeholder="Skriv navn på politiker..."
            value={searchText}
            onChange={handleSearchChange}
            disabled={isGuessing || isGameWon}
            className={polidleStyles.searchInput}
            autoComplete="off"
          />
          {searchText && selectedPoliticianId === null && (
            <>
              {isSearching && (
                <div className={polidleStyles.searchLoader}>Søger...</div>
              )}
              {searchError && (
                <div className={polidleStyles.searchError}>
                  Fejl: {searchError}
                </div>
              )}
              {!isSearching &&
                !searchError &&
                searchResults.length === 0 &&
                searchText.length > 0 && (
                  <div className={polidleStyles.noResults}>
                    Ingen match fundet.
                  </div>
                )}
              {!isSearching && !searchError && searchResults.length > 0 && (
                <ul className={polidleStyles.searchResults}>
                  {searchResults.map((option) => (
                    <li
                      key={option.id}
                      onClick={() => handleOptionSelect(option)}
                      className={polidleStyles.searchResultItem}
                    >
                      {option.pictureUrl ? (
                        <img
                          src={option.pictureUrl} // <<< BRUGER pictureUrl
                          alt={option.politikerNavn}
                          className={polidleStyles.searchResultImage}
                          loading="lazy"
                        />
                      ) : (
                        <div
                          className={polidleStyles.searchResultImagePlaceholder}
                        >
                          ?
                        </div>
                      )}
                      <span className={polidleStyles.searchResultName}>
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
            className={polidleStyles.guessButton}
          >
            {isGuessing ? "Gætter..." : "Gæt"}
          </button>
          {gameGuessError && (
            <div className={polidleStyles.guessError}>
              Fejl: {gameGuessError}
            </div>
          )}
        </div>
      )}
      {isGameWon && (
        <div className={polidleStyles.gameWonMessage}>
          <p>Godt gået! Du fandt politikeren!</p>
          <button onClick={resetGame} className={polidleStyles.playAgainButton}>
            {" "}
            {/* Opret denne CSS klasse */}
            Spil Igen
          </button>
        </div>
      )}
      {/* --------------------------- */}

      {/* --- Vis Gætte-Historik for Foto Mode --- */}
      <div className={polidleStyles.fotoGuessHistory}>
        {" "}
        {/* Opret evt. denne CSS klasse hvis forskellig fra citat */}
        {guessHistory.length > 0 && <h3>Dine Gæt:</h3>}
        {guessHistory.map(
          (
            item: FotoGuessHistoryDisplayItem,
            index // Brug DisplayItem type
          ) => (
            <div
              key={index}
              className={`${polidleStyles.citatGuessItem} ${
                // Genbruger citatGuessItem styling for nu
                item.isCorrect ? polidleStyles.correct : polidleStyles.incorrect
              }`}
            >
              {item.guessedInfo?.pictureUrl && ( // <<< BRUGER pictureUrl
                <img
                  src={item.guessedInfo.pictureUrl}
                  alt={item.guessedInfo.politikerNavn}
                  className={polidleStyles.historyImage}
                />
              )}
              <span className={polidleStyles.historyName}>
                {item.guessedInfo?.politikerNavn ?? "Ukendt"}
              </span>
              <span className={polidleStyles.historyIndicator}>
                {item.isCorrect ? "✓" : "✕"}
              </span>
            </div>
          )
        )}
      </div>
      {/* ------------------------------------------- */}
    </div>
  );
};

export default FotoBlurMode;
