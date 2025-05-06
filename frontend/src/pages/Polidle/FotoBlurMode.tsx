// Fil: src/pages/Polidle/FotoBlurMode.tsx (eller hvor den ligger)
import React, { useState, useEffect, useCallback, useRef } from "react";

// --- IMPORTER TYPER FRA DEN CENTRALE FIL ---
import {
  PoliticianOption,
  GuessRequestDto,
  GuessedPoliticianDetailsDto,
  GuessResultDto,
  PhotoDto, // Specifik for FotoMode
  GameMode,
  // Importer FeedbackType hvis du skal bruge den (f.eks. i CitatGuessHistoryItem hvis den genbruges)
} from "../../types/polidleTypes"; // <-- !! JUSTER STIEN !!

// Importer styles
import styles from "./Polidle.module.css"; // Generelle page styles
import polidleStyles from "../../components/Polidle/PolidleStyles.module.css"; // Styles til spillets UI-dele
import GameSelector from "../../components/Polidle/GamemodeSelector/GamemodeSelector";

// --- Type for historik i denne mode ---
interface FotoGuessHistoryItem {
  guessedInfo: GuessedPoliticianDetailsDto;
  isCorrect: boolean;
}

// --- Helper Funktion (Genbrugt - Flyt evt. til utils.ts) ---
function convertByteArrayToDataUrl(
  byteArray: number[],
  mimeType = "image/png"
): string {
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
const FotoBlurMode: React.FC = () => {
  // State for Foto
  const [photoBase64, setPhotoBase64] = useState<string | null>(null);
  const [isLoadingPhoto, setIsLoadingPhoto] = useState<boolean>(true);
  const [photoError, setPhotoError] = useState<string | null>(null);
  // const [blurLevel, setBlurLevel] = useState<number>(10); // State til blur-effekt

  // State for Politiker Søgning
  const [searchText, setSearchText] = useState<string>("");
  const [searchResults, setSearchResults] = useState<PoliticianOption[]>([]);
  const [isSearching, setIsSearching] = useState<boolean>(false);
  const [searchError, setSearchError] = useState<string | null>(null);
  const debounceTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  // State for Valgt Politiker
  const [selectedPoliticianId, setSelectedPoliticianId] = useState<
    number | null
  >(null);

  // State for Gæt Processering
  const [isGuessing, setIsGuessing] = useState<boolean>(false);
  const [guessError, setGuessError] = useState<string | null>(null);

  // State for Gæt Historik
  const [fotoGuesses, setFotoGuesses] = useState<FotoGuessHistoryItem[]>([]);

  // State for om spillet er vundet
  const [isGameWon, setIsGameWon] = useState<boolean>(false);

  // --- Effekt til at hente dagens foto ---
  useEffect(() => {
    const fetchPhoto = async () => {
      setIsLoadingPhoto(true);
      setPhotoError(null);
      const apiUrl = "/api/Polidle/photo/today"; // JUSTER evt. base URL
      try {
        const token = localStorage.getItem("jwt");
        const headers: HeadersInit = {
          "Content-Type":
            "application/json" /*...(token ? { 'Authorization': `Bearer ${token}` } : {})*/,
        };
        const response = await fetch(apiUrl, { headers });
        if (!response.ok) {
          throw new Error(
            `Fejl ${response.status}: Kunne ikke hente dagens foto.`
          );
        }
        const data: PhotoDto = await response.json();
        // Sammensæt fuld Data URL hvis backend kun sender Base64-delen
        if (
          data.portraitBase64 &&
          !data.portraitBase64.startsWith("data:image")
        ) {
          setPhotoBase64(`data:image/png;base64,${data.portraitBase64}`); // Antager png, juster hvis nødvendigt
        } else {
          setPhotoBase64(data.portraitBase64); // Antager backend sender fuld URL
        }
      } catch (error: any) {
        console.error("Fetch photo error:", error);
        setPhotoError(error.message || "Ukendt fejl ved hentning af foto.");
      } finally {
        setIsLoadingPhoto(false);
      }
    };
    fetchPhoto();
  }, []);

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
        const apiUrl = `/api/polidle/politicians?search=${encodedSearch}`; // JUSTER evt. base URL
        try {
          const token = localStorage.getItem("jwt");
          const headers: HeadersInit = {
            "Content-Type":
              "application/json" /*...(token ? { 'Authorization': `Bearer ${token}` } : {})*/,
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
    const gameModeValue = GameMode.Foto; // Brug enum (værdi 2)
    const apiUrl = "/api/polidle/guess"; // JUSTER evt. base URL
    const requestBody: GuessRequestDto = {
      guessedPoliticianId: selectedPoliticianId,
      gameMode: gameModeValue,
    };

    try {
      const token = localStorage.getItem("jwt");
      const headers: HeadersInit = {
        "Content-Type":
          "application/json" /*...(token ? { 'Authorization': `Bearer ${token}` } : {})*/,
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
          errorMsg = `${errorMsg} ${
            errorData.message || errorData.title || ""
          }`;
        } catch (e) {}
        throw new Error(errorMsg);
      }
      const resultData: GuessResultDto = await response.json();
      if (resultData.guessedPolitician) {
        const historyItem: FotoGuessHistoryItem = {
          guessedInfo: resultData.guessedPolitician,
          isCorrect: resultData.isCorrectGuess,
        };
        setFotoGuesses((prevGuesses) => [...prevGuesses, historyItem]); // Opdater Foto historik

        // TODO: Implementer blur reduktion ved forkert gæt
        // if (!resultData.isCorrectGuess) { setBlurLevel(prev => Math.max(0, prev - 2)); }

        if (resultData.isCorrectGuess) {
          setIsGameWon(true);
          // setBlurLevel(0); // Fjern blur
          // Vis evt. en pænere besked end alert
          setTimeout(() => alert("Tillykke, du gættede rigtigt!"), 100);
        }
        // TODO: Implementer max antal gæt logik hvis relevant
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
      <h1 className={styles.heading}>Polidle - Foto Mode</h1>
      <GameSelector />

      {/* --- Vis Foto --- */}
      <div className={polidleStyles.photoContainer}>
        <p className={styles.paragraph}>Hvem er på billedet?</p>
        {isLoadingPhoto && <p>Henter dagens billede...</p>}
        {photoError && (
          <p className={polidleStyles.errorText}>Fejl: {photoError}</p>
        )}
        {!isLoadingPhoto && !photoError && photoBase64 && (
          <img
            src={photoBase64}
            alt="Sløret politiker portræt"
            className={polidleStyles.blurredImage} // Tilføj CSS for denne klasse!
            // style={{ filter: `blur(${blurLevel}px)` }} // Tilføj når blur state er klar
          />
        )}
        {!isLoadingPhoto && !photoError && !photoBase64 && (
          <p style={{ color: "orange" }}>
            Kunne ikke finde et billede for i dag.
          </p>
        )}
      </div>
      {/* --------------- */}

      {/* --- Politiker Søgning/Valg --- */}
      {!isGameWon && (
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
              {!isSearching && !searchError && searchResults.length === 0 && (
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
                      <img
                        src={convertByteArrayToDataUrl(option.portraet)}
                        alt={option.politikerNavn}
                        className={polidleStyles.searchResultImage}
                        loading="lazy"
                      />
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
            onClick={handleMakeGuess}
            disabled={isGuessing || selectedPoliticianId === null || isGameWon}
            className={polidleStyles.guessButton}
          >
            {isGuessing ? "Gætter..." : "Gæt"}
          </button>
          {guessError && (
            <div className={polidleStyles.guessError}>Fejl: {guessError}</div>
          )}
        </div>
      )}
      {isGameWon && (
        <div className={polidleStyles.gameWonMessage}>
          Godt gået! Du fandt politikeren!
        </div>
      )}
      {/* --------------------------- */}

      {/* --- Vis Gætte-Historik for Foto Mode --- */}
      <div className={polidleStyles.fotoGuessHistory}>
        {fotoGuesses.length > 0 && <h3>Dine Gæt:</h3>}
        {fotoGuesses.map((guessItem, index) => (
          <div
            key={index}
            className={`${polidleStyles.citatGuessItem} ${
              guessItem.isCorrect
                ? polidleStyles.correcta
                : polidleStyles.incorrect
            }`}
          >
            {guessItem.guessedInfo?.portraet && (
              <img
                src={convertByteArrayToDataUrl(guessItem.guessedInfo.portraet)}
                alt={guessItem.guessedInfo.politikerNavn}
                className={polidleStyles.historyImage}
              />
            )}
            <span className={polidleStyles.historyName}>
              {guessItem.guessedInfo?.politikerNavn ?? "Ukendt"}
            </span>
            <span className={polidleStyles.historyIndicator}>
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
