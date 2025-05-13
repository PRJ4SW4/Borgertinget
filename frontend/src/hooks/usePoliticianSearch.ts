// src/hooks/usePoliticianSearch.ts
import { useState, useEffect, useRef, useCallback } from "react";
import { SearchListDto } from "../types/PolidleTypes"; // << VIGTIGT: Sørg for korrekt sti
import { fetchPoliticiansForSearch } from "../services/PolidleApiService"; // << VIGTIGT: Sørg for korrekt sti

interface UsePoliticianSearchReturn {
  searchText: string;
  setSearchText: React.Dispatch<React.SetStateAction<string>>;
  searchResults: SearchListDto[];
  isSearching: boolean;
  searchError: string | null;
  selectedPoliticianId: number | null;
  handleSearchChange: (event: React.ChangeEvent<HTMLInputElement>) => void;
  handleOptionSelect: (option: SearchListDto) => void;
  clearSelectionAndSearch: () => void; // Til at rydde efter et gæt
}

export const usePoliticianSearch = (
  debounceDelay: number = 300
): UsePoliticianSearchReturn => {
  const [searchText, setSearchText] = useState<string>("");
  const [searchResults, setSearchResults] = useState<SearchListDto[]>([]);
  const [isSearching, setIsSearching] = useState<boolean>(false);
  const [searchError, setSearchError] = useState<string | null>(null);
  const [selectedPoliticianId, setSelectedPoliticianId] = useState<
    number | null
  >(null);

  const debounceTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    if (debounceTimeoutRef.current) {
      clearTimeout(debounceTimeoutRef.current);
    }

    if (searchText.trim() === "" || selectedPoliticianId !== null) {
      setSearchResults([]);
      setIsSearching(false);
      return;
    }

    debounceTimeoutRef.current = setTimeout(async () => {
      if (searchText.trim() === "") {
        setSearchResults([]);
        setIsSearching(false);
        return;
      }

      setIsSearching(true);
      setSearchError(null);
      try {
        const data = await fetchPoliticiansForSearch(searchText);
        // Sikr at søgningen stadig er relevant (brugeren har ikke slettet alt imens)
        if (searchText.trim() !== "") {
          setSearchResults(data);
        } else {
          setSearchResults([]);
        }
      } catch (error) {
        console.error("Search fetch error in hook:", error);
        if (error instanceof Error) {
          setSearchError(error.message);
        } else {
          setSearchError("Fejl ved søgning af politikere.");
        }
        setSearchResults([]);
      } finally {
        setIsSearching(false);
      }
    }, debounceDelay);

    return () => {
      if (debounceTimeoutRef.current) {
        clearTimeout(debounceTimeoutRef.current);
      }
    };
  }, [searchText, selectedPoliticianId, debounceDelay]);

  const handleSearchChange = useCallback(
    (event: React.ChangeEvent<HTMLInputElement>) => {
      const newSearchText = event.target.value;
      setSearchText(newSearchText);
      setSelectedPoliticianId(null); // Ryd valgt politiker, da brugeren søger på ny
    },
    []
  );

  const handleOptionSelect = useCallback((option: SearchListDto) => {
    setSearchText(option.politikerNavn);
    setSelectedPoliticianId(option.id);
    setSearchResults([]); // Skjul søgeresultater
    setSearchError(null);
  }, []);

  const clearSelectionAndSearch = useCallback(() => {
    setSearchText("");
    setSelectedPoliticianId(null);
    setSearchResults([]);
    setSearchError(null);
  }, []);

  return {
    searchText,
    setSearchText, // Giver stadig direkte adgang hvis nødvendigt, men handleSearchChange er primær
    searchResults,
    isSearching,
    searchError,
    selectedPoliticianId,
    handleSearchChange,
    handleOptionSelect,
    clearSelectionAndSearch,
  };
};
