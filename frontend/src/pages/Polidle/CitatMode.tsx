// Fil: src/pages/Polidle/CitatMode.tsx (eller hvor den ligger)
import React, { useState, useEffect, useCallback, useRef } from "react";

// --- IMPORTER TYPER FRA DEN CENTRALE FIL ---
import {
  PoliticianOption,
  GuessRequestDto,
  GuessedPoliticianDetailsDto,
  GuessResultDto,
  QuoteDto, // Specifik for CitatMode
  GameMode,
} from "../../types/PolidleTypes"; // <-- !! VIGTIGT: Juster stien så den peger korrekt på din types fil !!

// Importer styles - opret/brug evt. separate filer
import styles from "./Polidle.module.css"; // Antager generelle page styles her
import polidleStyles from "../../components/Polidle/PolidleStyles.module.css"; // Styles til spillets UI-dele
import GameSelector from "../../components/Polidle/GamemodeSelector/GamemodeSelector";

// --- Type for historik i denne mode ---
interface CitatGuessHistoryItem {
  guessedInfo: GuessedPoliticianDetailsDto;
  isCorrect: boolean;
}

// --- Helper Funktion (Flyt evt. til en utils.ts fil) ---
function convertByteArrayToDataUrl(byteArray: number[], mimeType = "image/png"): string {
  if (!byteArray || byteArray.length === 0) return "placeholder.png"; // Sørg for at have en placeholder i /public mappen
  try {
    const uint8Array = new Uint8Array(byteArray);
    let binaryString = "";
    uint8Array.forEach((byte) => {
      binaryString += String.fromCharCode(byte);
    });
    const base64String = btoa(binaryString);
    return `data:${mimeType};base64,${base64String}`;
  } catch (error) {
    console.error("Error converting byte array:", error);
    return "placeholder.png";
  }
}

// --- Komponenten ---
const CitatMode: React.FC = () => {
  // State for Citat
  const [quote, setQuote] = useState<string | null>(null);
  const [isLoadingQuote, setIsLoadingQuote] = useState<boolean>(true);
  const [quoteError, setQuoteError] = useState<string | null>(null);

  // State for Politiker Søgning
  const [searchText, setSearchText] = useState<string>("");
  const [searchResults, setSearchResults] = useState<PoliticianOption[]>([]);
  const [isSearching, setIsSearching] = useState<boolean>(false);
  const [searchError, setSearchError] = useState<string | null>(null);
  const debounceTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  // State for Valgt Politiker
  const [selectedPoliticianId, setSelectedPoliticianId] = useState<number | null>(null);

  // State for Gæt Processering
  const [isGuessing, setIsGuessing] = useState<boolean>(false);
  const [guessError, setGuessError] = useState<string | null>(null);

  // State for Gæt Historik
  const [citatGuesses, setCitatGuesses] = useState<CitatGuessHistoryItem[]>([]);

  // State for om spillet er vundet
  const [isGameWon, setIsGameWon] = useState<boolean>(false);

  // --- Effekt til at hente dagens citat ---
  useEffect(() => {
    const fetchQuote = async () => {
      setIsLoadingQuote(true);
      setQuoteError(null);
      // JUSTER evt. base URL hvis din backend kører på en anden port
      const apiUrl = "/api/Polidle/quote/today";
      try {
        const token = localStorage.getItem("jwt");
        const headers: HeadersInit = {
          "Content-Type": "application/json" /*...(token ? { 'Authorization': `Bearer ${token}` } : {})*/,
        };
        const response = await fetch(apiUrl, { headers });
        if (!response.ok) {
          throw new Error(`Fejl ${response.status}`);
        }
        const data: QuoteDto = await response.json();
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

  // --- Effekt til Debounced Politiker Søgning ---
  useEffect(() => {
    if (debounceTimeoutRef.current) {
      clearTimeout(debounceTimeoutRef.current);
    }
    if (searchText.trim() === "" || selectedPoliticianId !== null) {
      setSearchResults([]);
      setIsSearching(false);
      return;
    }
    debounceTimeoutRef.current = setTimeout(() => {
      const fetchFilteredPoliticians = async () => {
        if (searchText.trim() === "") {
          setSearchResults([]);
          setIsSearching(false);
          return;
        }
        setIsSearching(true);
        setSearchError(null);
        const encodedSearch = encodeURIComponent(searchText);
        const apiUrl = `/api/polidle/politicians?search=${encodedSearch}`;
        try {
          const token = localStorage.getItem("jwt");
          const headers: HeadersInit = {
            "Content-Type": "application/json" /*...(token ? { 'Authorization': `Bearer ${token}` } : {})*/,
          };
          const response = await fetch(apiUrl, { headers });
          if (!response.ok) {
            throw new Error(`Fejl ${response.status}`);
          }
          const data: PoliticianOption[] = await response.json();
          if (searchText.trim() !== "") {
            setSearchResults(data);
          } else {
            setSearchResults([]);
          }
        } catch (error: any) {
          console.error("Search fetch error:", error);
          setSearchError(error.message || "Fejl ved søgning.");
          setSearchResults([]);
        } finally {
          setIsSearching(false);
        }
      };
      fetchFilteredPoliticians();
    }, 300);
    return () => {
      if (debounceTimeoutRef.current) {
        clearTimeout(debounceTimeoutRef.current);
      }
    };
  }, [searchText, selectedPoliticianId]);

  // --- Handlers ---
  const handleSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setSearchText(event.target.value);
    setSelectedPoliticianId(null);
  };

  const handleOptionSelect = (option: PoliticianOption) => {
    setSearchText(option.politikerNavn);
    setSelectedPoliticianId(option.id);
    setSearchResults([]);
    setSearchError(null);
  };

  const handleMakeGuess = async () => {
    if (selectedPoliticianId === null || isGameWon) {
      return;
    }
    setIsGuessing(true);
    setGuessError(null);
    const gameModeValue = GameMode.Citat; // Brug enum
    const apiUrl = "/api/polidle/guess";
    const requestBody: GuessRequestDto = {
      guessedPoliticianId: selectedPoliticianId,
      gameMode: gameModeValue,
    };

    try {
      const token = localStorage.getItem("jwt");
      const headers: HeadersInit = {
        "Content-Type": "application/json" /*...(token ? { 'Authorization': `Bearer ${token}` } : {})*/,
      };
      const response = await fetch(apiUrl, {
        method: "POST",
        headers: headers,
        body: JSON.stringify(requestBody),
      });
      if (!response.ok) {
        let errorMsg = `Fejl ${response.status}.`;
        try {
          const errorData = await response.json();
          errorMsg = `${errorMsg} ${errorData.message || errorData.title || ""}`;
        } catch (e) {}
        throw new Error(errorMsg);
      }
      const resultData: GuessResultDto = await response.json();
      if (resultData.guessedPolitician) {
        const historyItem: CitatGuessHistoryItem = {
          guessedInfo: resultData.guessedPolitician,
          isCorrect: resultData.isCorrectGuess,
        };
        setCitatGuesses((prevGuesses) => [...prevGuesses, historyItem]);
        if (resultData.isCorrectGuess) {
          setIsGameWon(true);
          // Udskift evt. alert med en pænere besked i UI
          setTimeout(() => alert("Tillykke, du gættede rigtigt!"), 100); // Lille delay så UI kan opdatere
        }
        // Tilføj evt. logik for max gæt
      } else {
        throw new Error("Manglende politiker detaljer i svar.");
      }
      setSearchText("");
      setSelectedPoliticianId(null);
      setSearchResults([]);
    } catch (error: any) {
      console.error("Guess API error:", error);
      setGuessError(error.message || "Ukendt fejl under gæt.");
    } finally {
      setIsGuessing(false);
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
        {quoteError && <p className={polidleStyles.errorText}>Fejl: {quoteError}</p>}
        {!isLoadingQuote && !quoteError && quote && <p className={styles.citat}>"{quote}"</p>}
        {!isLoadingQuote && !quoteError && !quote && <p style={{ color: "orange" }}>Kunne ikke finde et citat for i dag.</p>}
      </div>
      {/* --------------- */}

      {/* --- Politiker Søgning/Valg --- */}
      {!isGameWon && ( // Vis kun søgning hvis spillet ikke er vundet
        <div className={polidleStyles.searchContainer}>
          <input
            type="text"
            placeholder="Skriv navn på politiker..."
            value={searchText}
            onChange={handleSearchChange}
            disabled={isGuessing || isGameWon}
            className={polidleStyles.searchInput}
            autoComplete="off"
          />
          {/* Viser søgeresultater / loading / fejl */}
          {searchText && selectedPoliticianId === null && (
            <>
              {isSearching && <div className={polidleStyles.searchLoader}>Søger...</div>}
              {searchError && <div className={polidleStyles.searchError}>Fejl: {searchError}</div>}
              {!isSearching && !searchError && searchResults.length === 0 && <div className={polidleStyles.noResults}>Ingen match fundet.</div>}
              {!isSearching && !searchError && searchResults.length > 0 && (
                <ul className={polidleStyles.searchResults}>
                  {searchResults.map((option) => (
                    <li key={option.id} onClick={() => handleOptionSelect(option)} className={polidleStyles.searchResultItem}>
                      <img
                        src={convertByteArrayToDataUrl(option.portraet)}
                        alt={option.politikerNavn}
                        className={polidleStyles.searchResultImage}
                        loading="lazy"
                      />
                      <span className={polidleStyles.searchResultName}>{option.politikerNavn}</span>
                    </li>
                  ))}
                </ul>
              )}
            </>
          )}
          {/* Knap og Fejl for Gæt */}
          <button onClick={handleMakeGuess} disabled={isGuessing || selectedPoliticianId === null || isGameWon} className={polidleStyles.guessButton}>
            {isGuessing ? "Gætter..." : "Gæt"}
          </button>
          {guessError && <div className={polidleStyles.guessError}>Fejl: {guessError}</div>}
        </div>
      )}
      {/* Vis besked hvis spillet er vundet */}
      {isGameWon && <div className={polidleStyles.gameWonMessage}>Godt gået! Du fandt politikeren!</div>}
      {/* --------------------------- */}

      {/* --- Vis Gætte-Historik for Citat Mode --- */}
      <div className={polidleStyles.citatGuessHistory}>
        {citatGuesses.length > 0 && <h3>Dine Gæt:</h3>}
        {citatGuesses.map((guessItem, index) => (
          <div key={index} className={`${polidleStyles.citatGuessItem} ${guessItem.isCorrect ? polidleStyles.correct : polidleStyles.incorrect}`}>
            {/* Brug de CSS klasser du definerede */}
            {guessItem.guessedInfo?.portraet && (
              <img
                src={convertByteArrayToDataUrl(guessItem.guessedInfo.portraet)}
                alt={guessItem.guessedInfo.politikerNavn}
                className={polidleStyles.historyImage}
              />
            )}
            <span className={polidleStyles.historyName}>{guessItem.guessedInfo?.politikerNavn ?? "Ukendt"}</span>
            <span className={polidleStyles.historyIndicator}>{guessItem.isCorrect ? "✓" : "✕"}</span>
          </div>
        ))}
      </div>
      {/* ------------------------------------------- */}
    </div>
  );
};

export default CitatMode;
