// src/pages/Polidle/ClassicMode.tsx
import React from "react"; // Fjern ubrugte imports som useState, useEffect, useRef
import GuessList from "../../components/Polidle/GuessList/GuessList";
import Infobox from "../../components/Polidle/Infobox/Infobox";
import GameSelector from "../../components/Polidle/GamemodeSelector/GamemodeSelector";
import Input from "../../components/Polidle/Input/Input";

// Importer hooks
import { usePoliticianSearch } from "../../hooks/usePoliticianSearch"; // << VIGTIGT: Sørg for korrekt sti
import { useClassicPolidleGame } from "../../hooks/useClassicPolidleGame"; // << VIGTIGT: Sørg for korrekt sti

// Importer styles (som før)
import styles from "./Polidle.module.css";
import polidleStyles from "./Polidle.module.css"; // Stadig samme fil?

// --- Komponenten ---
const ClassicMode: React.FC = () => {
  const {
    searchText,
    searchResults,
    isSearching,
    searchError,
    selectedPoliticianId,
    handleSearchChange,
    handleOptionSelect,
    clearSelectionAndSearch, // Hentet fra hook
  } = usePoliticianSearch(); // Standard debounce delay bruges

  const {
    guessResults,
    isGuessing,
    guessError,
    makeGuess,
    // clearGuessHistory // Kan bruges hvis I vil have en "nulstil spil" knap
  } = useClassicPolidleGame();

  const handleGuessSubmit = async () => {
    if (selectedPoliticianId === null) return;

    const result = await makeGuess(selectedPoliticianId);
    if (result) {
      // Gæt blev forsøgt sendt
      clearSelectionAndSearch(); // Ryd søgefelt og valg efter gæt
      if (result.isCorrectGuess) {
        // Håndter "game won" UI her, f.eks. vis en besked, deaktiver input yderligere.
        // Dette kan også flyttes ind i useClassicPolidleGame hook'en hvis det er generisk nok.
        setTimeout(() => alert("Tillykke, du gættede rigtigt!"), 100);
      }
    }
  };

  // --- JSX Rendering ---
  return (
    <div className={styles.container}>
      <h1 className={styles.heading}>Polidle - Klassisk Mode</h1>
      <GameSelector />
      <p className={styles.paragraph}>Gæt dagens politiker</p>

      {/* --- Politiker Søgning/Valg --- */}
      <div className={polidleStyles.searchContainer}>
        <Input
          type="text"
          placeholder="Skriv navn på politiker..."
          value={searchText}
          onChange={handleSearchChange} // Brug fra hook
          disabled={isGuessing} // eller hvis spillet er vundet
          className={polidleStyles.searchInput}
          autoComplete="off"
        />
        {/* Vis kun listen hvis der er søgetekst OG en politiker IKKE er valgt endnu */}
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
              searchText.length > 0 && ( // Tilføjet searchText.length > 0 for ikke at vise "Ingen match" på tomt felt
                <div className={polidleStyles.noResults}>
                  Ingen match fundet.
                </div>
              )}
            {!isSearching && !searchError && searchResults.length > 0 && (
              <ul className={polidleStyles.searchResults}>
                {searchResults.map((option) => (
                  <li
                    key={option.id}
                    onClick={() => handleOptionSelect(option)} // Brug fra hook
                    className={polidleStyles.searchResultItem}
                  >
                    {/* OPDATERET: Bruger pictureUrl fra SearchListDto */}
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
                      </div> // Placeholder
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
          onClick={handleGuessSubmit} // Brug ny submit handler
          disabled={isGuessing || selectedPoliticianId === null}
          className={polidleStyles.guessButton}
        >
          {isGuessing ? "Gætter..." : "Gæt"}
        </button>
        {guessError && (
          <div className={polidleStyles.guessError}>Fejl: {guessError}</div>
        )}
      </div>
      {/* --------------------------- */}

      {/* --- Gætte-Liste --- */}
      <div className={polidleStyles.guessListContainer}>
        <GuessList results={guessResults} />
      </div>
      {/* ------------------ */}

      {/* Infobox */}
      <div className={polidleStyles.infoboxContainer}>
        <Infobox />
      </div>
    </div>
  );
};

export default ClassicMode;
