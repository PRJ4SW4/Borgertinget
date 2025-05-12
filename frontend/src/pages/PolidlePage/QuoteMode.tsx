// src/pages/Polidle/CitatMode.tsx
import React from "react"; // Kun React er nødvendig her
import GameSelector from "../../components/Polidle/GamemodeSelector/GamemodeSelector";
import Input from "../../components/Polidle/Input/Input";

// Importer hooks
import { usePoliticianSearch } from "../../hooks/usePoliticianSearch";
import { useCitatPolidleGame } from "../../hooks/useCitatPolidleGame";

// Importer typer til historik visning (hvis CitatGuessHistoryItem ikke er i hook'ens returtype)
import { DailyPoliticianDto } from "../../types/PolidleTypes";

// Importer styles
import styles from "./Polidle.module.css";
import polidleStyles from "../../components/Polidle/PolidleStyles.module.css";

// Type for historik i denne mode (hvis den skal være forskellig fra hook'ens interne)
// For nu bruger vi den samme, men den kan defineres specifikt her for komponenten, hvis nødvendigt.
interface CitatGuessHistoryDisplayItem {
  guessedInfo: DailyPoliticianDto; // Fra hook'ens CitatGuessHistoryItemInternal
  isCorrect: boolean;
}

// --- Komponenten ---
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
    guessHistory, // Hedder nu guessHistory
    isGuessing,
    guessError: gameGuessError, // Omdøb for at undgå navnekonflikt med searchError
    isGameWon,
    makeCitatGuess,
    resetGame, // Kan bruges til en "Nyt spil" knap
  } = useCitatPolidleGame();

  const handleGuessSubmit = async () => {
    if (selectedPoliticianId === null || isGameWon) return;

    const result = await makeCitatGuess(selectedPoliticianId);
    if (result) {
      clearSelectionAndSearch(); // Ryd søgefelt og valg efter gæt
      if (result.isCorrectGuess) {
        setTimeout(() => alert("Tillykke, du gættede rigtigt!"), 100);
      }
    }
  };

  // --- JSX Rendering ---
  return (
    <div className={styles.container}>
      <h1 className={styles.heading}>Polidle - Citat Mode</h1>
      <GameSelector />

      {/* --- Vis Citat --- */}
      <div className={polidleStyles.quoteContainer}>
        <p className={styles.paragraph}>Hvem sagde dette citat?</p>
        {isLoadingQuote && <p>Henter dagens citat...</p>}
        {quoteError && (
          <p className={polidleStyles.errorText}>Fejl: {quoteError}</p>
        )}
        {!isLoadingQuote && !quoteError && quote && (
          <p className={styles.citat}>"{quote}"</p>
        )}
        {!isLoadingQuote && !quoteError && !quote && (
          <p style={{ color: "orange" }}>
            Kunne ikke finde et citat for i dag. Prøv at genindlæse siden.
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
                          src={option.pictureUrl}
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
          {gameGuessError && ( // Brug omdøbt error state
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
            Spil Igen
          </button>
        </div>
      )}
      {/* --------------------------- */}

      {/* --- Vis Gætte-Historik for Citat Mode --- */}
      <div className={polidleStyles.citatGuessHistory}>
        {guessHistory.length > 0 && <h3>Dine Gæt:</h3>}
        {guessHistory.map((item, index) => (
          <div
            key={index} // Overvej et mere stabilt key, f.eks. item.guessedInfo.id + index hvis gæt kan gentages
            className={`${polidleStyles.citatGuessItem} ${
              item.isCorrect
                ? polidleStyles.correct // Sørg for at disse klasser findes i PolidleStyles.module.css
                : polidleStyles.incorrect
            }`}
          >
            {item.guessedInfo?.pictureUrl && ( // Tjekker for pictureUrl
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
        ))}
      </div>
      {/* ------------------------------------------- */}
    </div>
  );
};

export default CitatMode;
