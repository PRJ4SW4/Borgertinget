// Fil: src/pages/Polidle/ClassicMode/ClassicMode.tsx
import React, { useState, useCallback } from "react";
import GuessList from "../../components/Polidle/GuessList/GuessList"; // Opdateret sti
import Infobox from "../../components/Polidle/Infobox/Infobox"; // Opdateret sti
import GameSelector from "../../components/Polidle/GamemodeSelector/GamemodeSelector"; // Opdateret sti
import styles from "./Polidle.module.css"; // Generelle Polidle styles
import classicStyles from "./ClassicMode.module.css"; // Specifikke styles for ClassicMode (opret denne)

// Importer typer - bemærk at PoliticianOption nu hedder PoliticianSummaryDto
import {
  GuessResultDto,
  GuessRequestDto,
  PoliticianSummaryDto, // Opdateret type navn
  GameMode,
  FeedbackType, // Importer FeedbackType hvis den bruges direkte her
} from "../../types/polidleTypes";

// Importer custom hook og API service
import { usePoliticianSearch } from "../../hooks/usePoliticianSearch";
import { submitGuess } from "../../services/polidleApi";

// Importer utility funktion (hvis den stadig bruges her, ellers i SearchResultsList)
// import { convertByteArrayToDataUrl } from '../../../utils/imageUtils';

// --- Komponenten ---
const ClassicMode: React.FC = () => {
  // State specifikt for denne komponent
  const [searchText, setSearchText] = useState<string>(""); // Input feltets værdi
  const [selectedPolitician, setSelectedPolitician] =
    useState<PoliticianSummaryDto | null>(null);
  const [guessResults, setGuessResults] = useState<GuessResultDto[]>([]);
  const [isGuessing, setIsGuessing] = useState<boolean>(false);
  const [guessError, setGuessError] = useState<string | null>(null);

  // Brug custom hook til søgning
  const {
    searchResults,
    isLoading: isSearching, // Omdøb isLoading fra hook til isSearching for klarhed
    error: searchError,
    search: triggerSearch, // Omdøb search fra hook til triggerSearch
    clearResults: clearSearchResults, // Funktion til at rydde
  } = usePoliticianSearch(300); // 300ms debounce

  // Opdater søgetekst og trigger søgning via hook
  const handleSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newSearchText = event.target.value;
    setSearchText(newSearchText);
    setSelectedPolitician(null); // Ryd valgt politiker, når brugeren skriver
    if (newSearchText.trim() === "") {
      clearSearchResults(); // Ryd resultater hvis feltet tømmes
    } else {
      triggerSearch(newSearchText); // Start søgning via hook
    }
  };

  // Håndter valg fra søgeresultatlisten
  const handleOptionSelect = (option: PoliticianSummaryDto) => {
    setSearchText(option.politikerNavn); // Opdater input feltet
    setSelectedPolitician(option); // Sæt den valgte politiker
    clearSearchResults(); // Ryd søgeresultaterne (listen)
  };

  // Håndter afsendelse af gæt
  const handleMakeGuess = useCallback(async () => {
    if (!selectedPolitician) return; // Sikkerhedstjek

    setIsGuessing(true);
    setGuessError(null);

    const requestBody: GuessRequestDto = {
      guessedPoliticianId: selectedPolitician.id,
      gameMode: GameMode.Klassisk, // Brug enum
    };

    try {
      const resultData = await submitGuess(requestBody); // Brug API service funktion
      setGuessResults((prevResults) => [...prevResults, resultData]);

      // Ryd op efter succesfuldt gæt
      setSearchText("");
      setSelectedPolitician(null);
      clearSearchResults(); // Sørg for at søgeresultater også ryddes
    } catch (error: any) {
      console.error("Guess API error:", error);
      setGuessError(error.message || "Ukendt fejl under gæt.");
    } finally {
      setIsGuessing(false);
    }
  }, [selectedPolitician, clearSearchResults]); // Dependencies for useCallback

  // --- JSX Rendering ---
  return (
    <div className={styles.container}>
      {" "}
      {/* Brug generel container style */}
      <h1 className={styles.heading}>Polidle - Klassisk Mode</h1>
      <GameSelector /> {/* Viser gamemode vælger */}
      <p className={styles.paragraph}>Gæt dagens politiker</p>
      {/* --- Politiker Søgning/Valg --- */}
      {/* Overvej at lave dette til en separat <PoliticianSearchInput /> komponent */}
      <div className={classicStyles.searchAndGuess}>
        {" "}
        {/* Ny container for input + knap */}
        <div className={classicStyles.searchContainer}>
          {" "}
          {/* Container specifikt for input + resultater */}
          <input
            type="text"
            placeholder="Skriv navn på politiker..."
            value={searchText}
            onChange={handleSearchChange}
            disabled={isGuessing}
            className={classicStyles.searchInput}
            autoComplete="off"
          />
          {/* Vis kun listen hvis der er søgetekst OG en politiker IKKE er valgt */}
          {searchText && !selectedPolitician && (
            <div className={classicStyles.searchResultsContainer}>
              {" "}
              {/* Container for load/error/results */}
              {isSearching && (
                <div className={classicStyles.searchLoader}>Søger...</div>
              )}
              {searchError && (
                <div className={classicStyles.searchError}>
                  Fejl: {searchError}
                </div>
              )}
              {!isSearching &&
                !searchError &&
                searchResults.length === 0 &&
                searchText.trim() !== "" && (
                  <div className={classicStyles.noResults}>
                    Ingen match fundet.
                  </div>
                )}
              {!isSearching && !searchError && searchResults.length > 0 && (
                <ul className={classicStyles.searchResults}>
                  {searchResults.map((option) => (
                    <li
                      key={option.id}
                      onClick={() => handleOptionSelect(option)}
                      className={classicStyles.searchResultItem}
                      role="option" // For a11y
                      aria-selected={false}
                    >
                      {/* Billede fjernet herfra, da PoliticianSummaryDto ikke indeholder det */}
                      {/* <img src={placeholder_image} alt="" className={classicStyles.searchResultImage} /> */}
                      <span className={classicStyles.searchResultName}>
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
          disabled={isGuessing || !selectedPolitician} // Knappen er disabled hvis der ikke er valgt en politiker
          className={classicStyles.guessButton}
        >
          {isGuessing ? "Gætter..." : "Gæt"}
        </button>
      </div>
      {guessError && (
        <div className={classicStyles.guessError}>
          Fejl ved gæt: {guessError}
        </div>
      )}
      {/* --------------------------- */}
      {/* --- Gætte-Liste --- */}
      <div className={classicStyles.guessListContainer}>
        <GuessList results={guessResults} />
      </div>
      {/* ------------------ */}
      {/* --- Infobox --- */}
      <div className={classicStyles.infoboxContainer}>
        <Infobox />
      </div>
      {/* --------------- */}
    </div>
  );
};

export default ClassicMode;
