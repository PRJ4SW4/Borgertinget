import React, { useState, useEffect, useCallback, useRef } from "react"; // Importer mere
import GuessList from "../../components/Polidle/GuessList/GuessList"; // Importer igen
// import Infobox from "../../components/Polidle/Infobox/Infobox"; // Kan tilføjes senere
import styles from "./Polidle.module.css"; // Generelle styles
import polidleStyles from "../Polidle/Polidle.module.css"; // Specifikke Polidle komponent styles (opret denne fil)
import Infobox from "../../components/Polidle/Infobox/Infobox";
import GameSelector from "../../components/Polidle/GamemodeSelector/GamemodeSelector"; // Import GameSelector

// --- Interfaces/Types (Flyt evt. til en separat types.ts fil) ---
interface PoliticianOption {
  id: number;
  politikerNavn: string;
  portraet: number[]; // Byte array fra C#
}

// Matcher backend DTO
export enum FeedbackType { // Exporter så GuessItem kan bruge den
  Undefined = 0,
  Korrekt = 1,
  Forkert = 2,
  Højere = 3,
  Lavere = 4,
}
export interface GuessedPoliticianDetailsDto {
  // Exporter
  id: number;
  politikerNavn: string;
  partiNavn: string;
  alder: number;
  køn: string;
  uddannelse: string;
  region: string;
}
export interface GuessResultDto {
  // Exporter
  isCorrectGuess: boolean;
  feedback: { [key: string]: FeedbackType }; // Record<string, FeedbackType>
  guessedPolitician: GuessedPoliticianDetailsDto | null; // Gør den klar til brug
}
// Til request
interface GuessRequestDto {
  guessedPoliticianId: number;
  gameMode: number | string; // Afhænger af din backend (0 eller "Klassisk")
}

// --- Helper Funktion ---
function convertByteArrayToDataUrl(
  byteArray: number[],
  mimeType = "image/png"
): string {
  // ... (samme funktion som før) ...
  if (!byteArray || byteArray.length === 0) return "placeholder.png";
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
const ClassicMode: React.FC = () => {
  const [searchText, setSearchText] = useState<string>("");
  const [searchResults, setSearchResults] = useState<PoliticianOption[]>([]);
  const [isSearching, setIsSearching] = useState<boolean>(false);
  const [searchError, setSearchError] = useState<string | null>(null);

  const [selectedPoliticianId, setSelectedPoliticianId] = useState<
    number | null
  >(null);
  // Fjernet selectedPoliticianName, da searchText bruges til at vise det valgte navn i input

  const [guessResults, setGuessResults] = useState<GuessResultDto[]>([]);
  const [isGuessing, setIsGuessing] = useState<boolean>(false);
  const [guessError, setGuessError] = useState<string | null>(null);

  // Ref til at holde timeout ID for debouncing
  const debounceTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  // --- Debounced API kald til at søge ---
  useEffect(() => {
    // Ryd tidligere timeout hvis brugeren taster videre
    if (debounceTimeoutRef.current) {
      clearTimeout(debounceTimeoutRef.current);
    }

    // Hvis søgefeltet er tomt eller en politiker *er* valgt, ryd resultater og stop
    if (searchText.trim() === "" || selectedPoliticianId !== null) {
      setSearchResults([]);
      setIsSearching(false); // Sørg for at loading stoppes
      return;
    }

    // Start en ny timeout
    debounceTimeoutRef.current = setTimeout(() => {
      const fetchFilteredPoliticians = async () => {
        // Tjek igen hvis brugeren slettede tekst imens timeren kørte
        if (searchText.trim() === "") {
          setSearchResults([]);
          setIsSearching(false);
          return;
        }

        setIsSearching(true);
        setSearchError(null);
        const encodedSearch = encodeURIComponent(searchText);
        const apiUrl = `/api/polidle/politicians?search=${encodedSearch}`; // Din backend URL

        try {
          const token = localStorage.getItem("jwt");
          const headers: HeadersInit = {
            "Content-Type":
              "application/json" /*...(token ? { 'Authorization': `Bearer ${token}` } : {})*/,
          }; // Afkommenter auth hvis nødvendigt
          const response = await fetch(apiUrl, { headers });

          if (!response.ok) {
            throw new Error(`Fejl ${response.status}`);
          }

          const data: PoliticianOption[] = await response.json();
          // Vis kun resultater hvis søgeteksten *stadig* er relevant
          // (brugeren kan have slettet/ændret teksten mens fetch kørte)
          // Sammenlign med den searchText der udløste kaldet (kræver lidt mere state/ref)
          // Simpel løsning: bare opdater altid hvis der er søgetekst
          if (searchText.trim() !== "") {
            setSearchResults(data);
          } else {
            setSearchResults([]); // Ryd hvis søgetekst er blevet fjernet imens
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
    }, 300); // 300ms debounce

    // Cleanup funktion der rydder timeout hvis component unmountes
    return () => {
      if (debounceTimeoutRef.current) {
        clearTimeout(debounceTimeoutRef.current);
      }
    };
  }, [searchText, selectedPoliticianId]); // Kør effekten igen hvis searchText ændres ELLER en politiker vælges (for at rydde listen)

  // Opdater søgetekst
  const handleSearchChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newSearchText = event.target.value;
    setSearchText(newSearchText);
    // Ryd valgt politiker ID, da brugeren søger på ny
    setSelectedPoliticianId(null);
  };

  // Håndter valg fra listen
  const handleOptionSelect = (option: PoliticianOption) => {
    setSearchText(option.politikerNavn); // Sæt input-tekst til valgt navn
    setSelectedPoliticianId(option.id); // Gem valgt ID
    setSearchResults([]); // Skjul listen
    setSearchError(null);
  };

  // Håndter gæt (API kald til backend)
  const handleMakeGuess = async () => {
    if (selectedPoliticianId === null) {
      /* Burde ikke ske pga. disabled knap */ return;
    }
    setIsGuessing(true);
    setGuessError(null);
    const gameModeValue: number | string = 0; // Eller "Klassisk"
    const apiUrl = "/api/polidle/guess";
    const requestBody: GuessRequestDto = {
      guessedPoliticianId: selectedPoliticianId,
      gameMode: gameModeValue,
    };

    try {
      const token = localStorage.getItem("jwt");
      const headers: HeadersInit = {
        "Content-Type":
          "application/json" /*...(token ? { 'Authorization': `Bearer ${token}` } : {})*/,
      }; // Afkommenter auth hvis nødvendigt
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
      setGuessResults((prevResults) => [...prevResults, resultData]); // Tilføj til listen af resultater
      // Ryd søgefelt og valg
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
      <h1 className={styles.heading}>Polidle - Klassisk Mode</h1>
      <GameSelector />
      <p className={styles.paragraph}>Gæt dagens politiker</p>
      {/* <GameSelector /> */};{/* --- Politiker Søgning/Valg --- */}
      <div className={polidleStyles.searchContainer}>
        <input
          type="text"
          placeholder="Skriv navn på politiker..."
          value={searchText}
          onChange={handleSearchChange}
          disabled={isGuessing}
          className={polidleStyles.searchInput}
          autoComplete="off" // Undgå browser autocomplete
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
            {!isSearching && !searchError && searchResults.length === 0 && (
              <div className={polidleStyles.noResults}>Ingen match fundet.</div>
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
                      className={polidleStyles.searchResultImage} // Style denne!
                      loading="lazy" // Forsink indlæsning af billeder i listen
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
        {/* Her indsættes den OPDATEREDE GuessList */}
        <GuessList results={guessResults} />
        {/* Midlertidig visning fjernet - vi bruger den rigtige nu */}
      </div>
      {/* ------------------ */}
      {/* <Infobox /> */}
      {/* Infobox */}
      <div className={polidleStyles.infoboxContainer}>
        <Infobox />
      </div>
    </div>
  );
};

export default ClassicMode;
