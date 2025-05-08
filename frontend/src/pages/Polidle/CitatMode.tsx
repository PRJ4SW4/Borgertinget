// Fil: src/pages/Polidle/CitatMode/CitatMode.tsx
import React, { useState, useEffect, useCallback } from "react";
// Importer nødvendige komponenter
import GameSelector from "../../components/Polidle/GamemodeSelector/GamemodeSelector";
// Importer styles
import styles from "../Polidle.module.css"; // Generelle Polidle styles
import citatStyles from "./CitatMode.module.css"; // Specifikke styles for CitatMode (opret denne)

// Importer typer
import {
  PoliticianSummaryDto,
  GuessRequestDto,
  GuessResultDto,
  QuoteDto,
  GameMode,
  GuessHistoryItem, // Brug den generiske historik type
} from "../../types/polidleTypes";

// Importer custom hook og API service funktioner
import { usePoliticianSearch } from "../../hooks/usePoliticianSearch";
import { submitGuess, getQuoteOfTheDay } from "../../services/polidleApi";

// Fjernet unødvendig import af imageUtils, da portræt ikke vises i historik her
// import { convertByteArrayToDataUrl } from '../../../utils/imageUtils';

// --- Komponenten ---
// Fjernet CitatModeProps, da citat hentes internt
const CitatMode: React.FC = () => {
  // State for Citat
  const [quote, setQuote] = useState<string | null>(null);
  const [isLoadingQuote, setIsLoadingQuote] = useState<boolean>(true);
  const [quoteError, setQuoteError] = useState<string | null>(null);

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

  // Brug custom hook til søgning
  const {
    searchResults,
    isLoading: isSearching,
    error: searchError,
    search: triggerSearch,
    clearResults: clearSearchResults,
  } = usePoliticianSearch(300);

  // --- Effekt til at hente dagens citat ---
  useEffect(() => {
    const fetchQuote = async () => {
      setIsLoadingQuote(true);
      setQuoteError(null);
      try {
        const data = await getQuoteOfTheDay(); // Brug API service funktion
        setQuote(data.quoteText);
      } catch (error: any) {
        console.error("Fetch quote error:", error);
        setQuoteError(error.message || "Ukendt fejl ved hentning af citat.");
      } finally {
        setIsLoadingQuote(false);
      }
    };
    fetchQuote();
  }, []); // Kør kun ved mount

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
      gameMode: GameMode.Citat, // Brug enum
    };

    try {
      const resultData = await submitGuess(requestBody);

      if (resultData.guessedPolitician) {
        const historyItem: GuessHistoryItem = {
          // Brug GuessHistoryItem
          guessedInfo: resultData.guessedPolitician,
          isCorrect: resultData.isCorrectGuess,
          // Feedback er ikke relevant for CitatMode historik
        };
        setGuessHistory((prevGuesses) => [...prevGuesses, historyItem]);

        if (resultData.isCorrectGuess) {
          setIsGameWon(true);
          // Overvej en mere integreret success-besked end alert
          // f.eks. vis en besked i UI'en
        }
        // Tilføj evt. logik for max antal gæt her
      } else {
        // Dette burde ikke ske hvis API'et altid returnerer guessedPolitician
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
      <h1 className={styles.heading}>Polidle - Citat Mode</h1>
      <GameSelector />

      {/* --- Vis Citat --- */}
      <div className={citatStyles.quoteContainer}>
        <p className={styles.paragraph}>Hvem sagde dette citat?</p>
        {isLoadingQuote && (
          <p className={citatStyles.loadingText}>Henter dagens citat...</p>
        )}
        {quoteError && (
          <p className={citatStyles.errorText}>Fejl: {quoteError}</p>
        )}
        {!isLoadingQuote && !quoteError && quote && (
          // Brug blockquote for semantisk korrekt HTML for citater
          <blockquote className={citatStyles.quote}>"{quote}"</blockquote>
        )}
        {!isLoadingQuote && !quoteError && !quote && (
          <p className={citatStyles.warningText}>
            Kunne ikke finde et citat for i dag.
          </p>
        )}
      </div>
      {/* --------------- */}

      {/* --- Politiker Søgning/Valg (kun hvis spillet ikke er vundet) --- */}
      {!isGameWon && (
        <div className={citatStyles.searchAndGuess}>
          {" "}
          {/* Ny container for input + knap */}
          <div className={citatStyles.searchContainer}>
            {" "}
            {/* Container specifikt for input + resultater */}
            <input
              type="text"
              placeholder="Skriv navn på politiker..."
              value={searchText}
              onChange={handleSearchChange}
              disabled={isGuessing || isGameWon}
              className={citatStyles.searchInput}
              autoComplete="off"
            />
            {/* Vis kun listen hvis der er søgetekst OG en politiker IKKE er valgt */}
            {searchText && !selectedPolitician && (
              <div className={citatStyles.searchResultsContainer}>
                {isSearching && (
                  <div className={citatStyles.searchLoader}>Søger...</div>
                )}
                {searchError && (
                  <div className={citatStyles.searchError}>
                    Fejl: {searchError}
                  </div>
                )}
                {!isSearching &&
                  !searchError &&
                  searchResults.length === 0 &&
                  searchText.trim() !== "" && (
                    <div className={citatStyles.noResults}>
                      Ingen match fundet.
                    </div>
                  )}
                {!isSearching && !searchError && searchResults.length > 0 && (
                  <ul className={citatStyles.searchResults}>
                    {searchResults.map((option) => (
                      <li
                        key={option.id}
                        onClick={() => handleOptionSelect(option)}
                        className={citatStyles.searchResultItem}
                        role="option"
                        aria-selected={false}
                      >
                        {/* Billede fjernet - ikke i PoliticianSummaryDto */}
                        <span className={citatStyles.searchResultName}>
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
            className={citatStyles.guessButton}
          >
            {isGuessing ? "Gætter..." : "Gæt"}
          </button>
        </div>
      )}
      {guessError &&
        !isGameWon && ( // Vis kun gættefejl hvis spillet ikke er slut
          <div className={citatStyles.guessError}>
            Fejl ved gæt: {guessError}
          </div>
        )}

      {/* Vis besked hvis spillet er vundet */}
      {isGameWon && (
        <div className={citatStyles.gameWonMessage}>
          Tillykke! Du gættede rigtigt!
        </div>
      )}
      {/* --------------------------- */}

      {/* --- Vis Gætte-Historik for Citat Mode --- */}
      <div className={citatStyles.guessHistory}>
        {guessHistory.length > 0 && <h3>Dine Gæt:</h3>}
        {guessHistory.map((guessItem, index) => (
          <div
            key={`${guessItem.guessedInfo.id}-${index}`} // Bedre key end bare index
            className={`${citatStyles.guessHistoryItem} ${
              guessItem.isCorrect ? citatStyles.correct : citatStyles.incorrect
            }`}
          >
            {/* Billede fjernet - GuessedPoliticianDetailsDto har ikke portræt */}
            {/* <img src={placeholder} alt="" className={citatStyles.historyImage} /> */}
            <span className={citatStyles.historyName}>
              {guessItem.guessedInfo?.politikerNavn ?? "Ukendt"}
            </span>
            <span className={citatStyles.historyIndicator}>
              {guessItem.isCorrect ? "✓" : "✕"}
            </span>
          </div>
        ))}
      </div>
      {/* ------------------------------------------- */}
    </div>
  );
};

export default CitatMode;
