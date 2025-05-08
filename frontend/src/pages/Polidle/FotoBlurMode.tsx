// Fil: src/pages/Polidle/FotoBlurMode/FotoBlurMode.tsx
import React, { useState, useCallback } from "react";
// Importer nødvendige komponenter
import GameSelector from "../../components/Polidle/GamemodeSelector/GamemodeSelector";
// Importer styles
import styles from "../Polidle.module.css"; // Generelle Polidle styles
import fotoStyles from "./FotoBlurMode.module.css"; // Specifikke styles for FotoBlurMode (opret denne)

// Importer typer
import {
  PoliticianSummaryDto,
  GuessRequestDto,
  GuessResultDto,
  PhotoDto,
  GameMode,
  GuessHistoryItem, // Brug den generiske historik type
} from "../../types/polidleTypes";

// Importer custom hooks og API service funktioner
import { usePoliticianSearch } from "../../hooks/usePoliticianSearch";
import { useDailyPhoto } from "../../hooks/useDailyPhoto"; // Ny hook
import { submitGuess } from "../../services/polidleApi";

// Importer utility funktion til at vise Base64 billeder
import { convertBase64ToDataUrl } from "../../utils/polidleHelpers";

// --- Komponenten ---
// Fjernet FotoBlurModeProps, da foto hentes internt via hook
const FotoBlurMode: React.FC = () => {
  // State for Foto (bruger hook)
  const {
    photoData,
    isLoading: isLoadingPhoto,
    error: photoError,
    retry: retryFetchPhoto,
  } = useDailyPhoto();

  // const [blurLevel, setBlurLevel] = useState<number>(10); // State til blur-effekt (stadig kommenteret ud)

  // State for Politiker Søgning (bruger hook)
  const [searchText, setSearchText] = useState<string>("");
  const [selectedPolitician, setSelectedPolitician] =
    useState<PoliticianSummaryDto | null>(null);

  // State for Gæt Processering
  const [isGuessing, setIsGuessing] = useState<boolean>(false);
  const [guessError, setGuessError] = useState<string | null>(null);

  // State for Gæt Historik (bruger GuessHistoryItem)
  const [guessHistory, setGuessHistory] = useState<GuessHistoryItem[]>([]);

  // State for om spillet er vundet
  const [isGameWon, setIsGameWon] = useState<boolean>(false);

  // Brug custom hook til politikersøgning
  const {
    searchResults,
    isLoading: isSearching,
    error: searchError,
    search: triggerSearch,
    clearResults: clearSearchResults,
  } = usePoliticianSearch(300);

  // --- Handlers ---
  const handleSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newSearchText = event.target.value;
    setSearchText(newSearchText);
    setSelectedPolitician(null);
    if (newSearchText.trim() === "") {
      clearSearchResults();
    } else {
      triggerSearch(newSearchText);
    }
  };

  const handleOptionSelect = (option: PoliticianSummaryDto) => {
    setSearchText(option.politikerNavn);
    setSelectedPolitician(option);
    clearSearchResults();
  };

  const handleMakeGuess = useCallback(async () => {
    if (!selectedPolitician || isGameWon) return;

    setIsGuessing(true);
    setGuessError(null);

    const requestBody: GuessRequestDto = {
      guessedPoliticianId: selectedPolitician.id,
      gameMode: GameMode.Foto, // Brug enum
    };

    try {
      const resultData = await submitGuess(requestBody);

      if (resultData.guessedPolitician) {
        const historyItem: GuessHistoryItem = {
          guessedInfo: resultData.guessedPolitician,
          isCorrect: resultData.isCorrectGuess,
        };
        setGuessHistory((prevGuesses) => [...prevGuesses, historyItem]);

        // TODO: Implementer blur reduktion ved forkert gæt
        // if (!resultData.isCorrectGuess) { setBlurLevel(prev => Math.max(0, prev - 2)); }

        if (resultData.isCorrectGuess) {
          setIsGameWon(true);
          // setBlurLevel(0); // Fjern blur ved korrekt gæt
          // Overvej en mere integreret success-besked
        }
        // TODO: Implementer max antal gæt logik
      } else {
        console.error("API response missing 'guessedPolitician' details.");
        throw new Error("Manglende politiker detaljer i svar fra server.");
      }

      // Ryd op efter gæt
      setSearchText("");
      setSelectedPolitician(null);
      clearSearchResults();
    } catch (error: any) {
      console.error("Guess API error:", error);
      setGuessError(error.message || "Ukendt fejl under gæt.");
    } finally {
      setIsGuessing(false);
    }
  }, [selectedPolitician, isGameWon, clearSearchResults]); // Dependencies

  // --- JSX Rendering ---
  return (
    <div className={styles.container}>
      <h1 className={styles.heading}>Polidle - Foto Mode</h1>
      <GameSelector />

      {/* --- Vis Foto --- */}
      <div className={fotoStyles.photoContainer}>
        <p className={styles.paragraph}>Hvem er på billedet?</p>
        {isLoadingPhoto && (
          <p className={fotoStyles.loadingText}>Henter dagens billede...</p>
        )}
        {photoError && (
          <div className={fotoStyles.errorContainer}>
            <p className={fotoStyles.errorText}>Fejl: {photoError}</p>
            <button
              onClick={retryFetchPhoto}
              className={fotoStyles.retryButton}
            >
              Prøv igen
            </button>
          </div>
        )}
        {!isLoadingPhoto && !photoError && photoData?.portraitBase64 && (
          <img
            // Brug convertBase64ToDataUrl da PhotoDto indeholder Base64 streng
            src={convertBase64ToDataUrl(photoData.portraitBase64)}
            alt="Sløret politiker portræt"
            className={fotoStyles.blurredImage} // Sørg for at denne klasse findes i CSS
            // style={{ filter: `blur(${blurLevel}px)` }} // Tilføj når blur state implementeres
          />
        )}
        {!isLoadingPhoto && !photoError && !photoData?.portraitBase64 && (
          // Viser dette hvis API'et returnerede data, men base64 strengen var tom
          <p className={fotoStyles.warningText}>
            Kunne ikke finde et billede for i dag.
          </p>
        )}
      </div>
      {/* --------------- */}

      {/* --- Politiker Søgning/Valg (kun hvis spillet ikke er vundet) --- */}
      {!isGameWon && (
        <div className={fotoStyles.searchAndGuess}>
          <div className={fotoStyles.searchContainer}>
            <input
              type="text"
              placeholder="Skriv navn på politiker..."
              value={searchText}
              onChange={handleSearchChange}
              disabled={isGuessing || isGameWon}
              className={fotoStyles.searchInput}
              autoComplete="off"
            />
            {searchText && !selectedPolitician && (
              <div className={fotoStyles.searchResultsContainer}>
                {isSearching && (
                  <div className={fotoStyles.searchLoader}>Søger...</div>
                )}
                {searchError && (
                  <div className={fotoStyles.searchError}>
                    Fejl: {searchError}
                  </div>
                )}
                {!isSearching &&
                  !searchError &&
                  searchResults.length === 0 &&
                  searchText.trim() !== "" && (
                    <div className={fotoStyles.noResults}>
                      Ingen match fundet.
                    </div>
                  )}
                {!isSearching && !searchError && searchResults.length > 0 && (
                  <ul className={fotoStyles.searchResults}>
                    {searchResults.map((option) => (
                      <li
                        key={option.id}
                        onClick={() => handleOptionSelect(option)}
                        className={fotoStyles.searchResultItem}
                        role="option"
                        aria-selected={false}
                      >
                        {/* Billede fjernet */}
                        <span className={fotoStyles.searchResultName}>
                          {option.politikerNavn}
                        </span>
                      </li>
                    ))}
                  </ul>
                )}
              </div>
            )}
          </div>

          <button
            onClick={handleMakeGuess}
            disabled={isGuessing || !selectedPolitician || isGameWon}
            className={fotoStyles.guessButton}
          >
            {isGuessing ? "Gætter..." : "Gæt"}
          </button>
        </div>
      )}
      {guessError && !isGameWon && (
        <div className={fotoStyles.guessError}>Fejl ved gæt: {guessError}</div>
      )}

      {/* Vis besked hvis spillet er vundet */}
      {isGameWon && (
        <div className={fotoStyles.gameWonMessage}>
          Tillykke! Du gættede rigtigt!
        </div>
      )}
      {/* --------------------------- */}

      {/* --- Vis Gætte-Historik for Foto Mode --- */}
      <div className={fotoStyles.guessHistory}>
        {guessHistory.length > 0 && <h3>Dine Gæt:</h3>}
        {guessHistory.map((guessItem, index) => (
          <div
            key={`${guessItem.guessedInfo.id}-${index}`}
            className={`${fotoStyles.guessHistoryItem} ${
              // Brug evt. samme styles som Citat?
              guessItem.isCorrect ? fotoStyles.correct : fotoStyles.incorrect
            }`}
          >
            {/* Billede fjernet */}
            <span className={fotoStyles.historyName}>
              {guessItem.guessedInfo?.politikerNavn ?? "Ukendt"}
            </span>
            <span className={fotoStyles.historyIndicator}>
              {guessItem.isCorrect ? "✓" : "✕"}
            </span>
          </div>
        ))}
      </div>
      {/* ------------------------------------------- */}
    </div>
  );
};

export default FotoBlurMode;
