// src/pages/Polidle/QuoteMode.tsx
import React from "react";
import GameSelector from "../../../components/Polidle/GamemodeSelector/GamemodeSelector";
import Input from "../../../components/Polidle/Input/Input";
import { DailyPoliticianDto, SearchListDto } from "../../../types/PolidleTypes"; // SearchListDto kan være nødvendig for searchResults typen

import { useQuoteMode } from "./QuoteMode.logic"; // <<< Importer den nye hook
import pageStyles from "./QuoteMode.module.css";
import sharedStyles from "../../../components/Polidle/SharedPolidle.module.css";

// Dette interface bruges til at definere strukturen for gættehistorik-elementer i denne komponent.
// DailyPoliticianDto er nødvendig, da guessHistory-items fra hook'en forventes at indeholde `guessedInfo` af denne type.
interface CitatGuessHistoryDisplayItem {
  guessedInfo: DailyPoliticianDto;
  isCorrect: boolean;
}

const QuoteMode: React.FC = () => {
  const {
    searchText,
    searchResults, // Typen afhænger af hvad usePoliticianSearch (via useQuoteMode) returnerer. Antaget SearchListDto[]
    isSearching,
    searchError,
    selectedPoliticianId, // Bruges til at styre 'disabled' på gætte-knappen
    handleSearchChange,
    handleOptionSelect,
    // setSearchText, // Kun nødvendig hvis du har brug for at sætte searchText direkte udenom handleSearchChange/handleOptionSelect

    quote,
    isLoadingQuote,
    quoteError,
    guessHistory, // Forventes at være CitatGuessHistoryDisplayItem[] eller kompatibel
    isGuessing,
    gameGuessError,
    isGameWon,
    handleGuessSubmit,
    resetGame,
  } = useQuoteMode(); // <<< Brug den nye hook

  // Sideeffekter som f.eks. en alert ved spil vundet kan håndteres her med useEffect, hvis det ønskes.
  // F.eks.:
  // useEffect(() => {
  //   if (isGameWon) {
  //     alert("Tillykke, du gættede rigtigt! (Citat Mode)");
  //   }
  // }, [isGameWon]);
  // Ofte er den besked, der vises i JSX'en, når isGameWon er true, tilstrækkelig.

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

      {/* Søge- og gætte-sektion */}
      {!isGameWon && (
        <div className={sharedStyles.searchContainer}>
          <Input
            type="text"
            placeholder="Skriv navn på politiker..."
            value={searchText}
            onChange={handleSearchChange}
            disabled={isGuessing || isGameWon} // Input deaktiveres under gæt eller hvis spillet er vundet
            className={sharedStyles.searchInput}
            autoComplete="off"
          />
          {/* Søgeresultater vises kun hvis der er søgetekst OG ingen politiker er valgt endnu */}
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
                  {searchResults.map(
                    (
                      option: SearchListDto // Antager option er af typen SearchListDto
                    ) => (
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
                            className={
                              sharedStyles.searchResultImagePlaceholder
                            }
                          >
                            ?
                          </div>
                        )}
                        <span className={sharedStyles.searchResultName}>
                          {option.politikerNavn}
                        </span>
                      </li>
                    )
                  )}
                </ul>
              )}
            </>
          )}
          <button
            onClick={handleGuessSubmit}
            disabled={isGuessing || selectedPoliticianId === null || isGameWon} // Knappen er deaktiveret hvis der gættes, ingen er valgt, eller spillet er vundet
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

      {/* Gættehistorik */}
      <div className={sharedStyles.citatGuessHistory}>
        {guessHistory.length > 0 && (
          <h3 className={sharedStyles.historyHeader}>Dine Gæt:</h3>
        )}
        {/* Mapper over guessHistory. CitatGuessHistoryDisplayItem definerer formen på 'item'. */}
        {guessHistory.map((item: CitatGuessHistoryDisplayItem, index) => (
          <div
            key={index} // Overvej en mere stabil key, f.eks. item.guessedInfo.id hvis tilgængelig og unik for hvert gæt
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

export default QuoteMode;
