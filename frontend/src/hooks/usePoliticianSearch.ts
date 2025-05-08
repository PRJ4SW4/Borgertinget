// Fil: src/hooks/usePoliticianSearch.ts
import { useState, useEffect, useRef, useCallback } from "react";
import { PoliticianSummaryDto } from "../types/polidleTypes"; // Importer type
import { searchPoliticians } from "../services/polidleApi"; // Importer API funktion

interface UsePoliticianSearchResult {
  searchResults: PoliticianSummaryDto[];
  isLoading: boolean;
  error: string | null;
  search: (term: string) => void; // Funktion til at starte søgning
  clearResults: () => void; // Funktion til at rydde resultater manuelt
}

/**
 * Custom hook to handle debounced politician search.
 * @param debounceMs Debounce time in milliseconds (default: 300ms).
 * @returns State and functions for searching politicians.
 */
export function usePoliticianSearch(
  debounceMs: number = 300
): UsePoliticianSearchResult {
  const [searchTerm, setSearchTerm] = useState<string>("");
  const [searchResults, setSearchResults] = useState<PoliticianSummaryDto[]>(
    []
  );
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const debounceTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const mountedRef = useRef<boolean>(true); // Track if component is mounted

  // Set mountedRef to false on unmount to prevent state updates
  useEffect(() => {
    mountedRef.current = true;
    return () => {
      mountedRef.current = false;
      // Clear timeout on unmount
      if (debounceTimeoutRef.current) {
        clearTimeout(debounceTimeoutRef.current);
      }
    };
  }, []);

  // Debounced effect to fetch results
  useEffect(() => {
    // Clear previous timeout
    if (debounceTimeoutRef.current) {
      clearTimeout(debounceTimeoutRef.current);
    }

    if (!searchTerm.trim()) {
      setSearchResults([]); // Clear results if search term is empty
      setIsLoading(false);
      setError(null);
      return;
    }

    // Start new timeout
    debounceTimeoutRef.current = setTimeout(async () => {
      if (!mountedRef.current) return; // Don't fetch if unmounted

      setIsLoading(true);
      setError(null);
      try {
        const data = await searchPoliticians(searchTerm);
        if (mountedRef.current) {
          // Check again before setting state
          // Only update results if the search term hasn't changed drastically while fetching
          // This check might need refinement depending on exact desired behavior
          if (searchTerm.trim() !== "") {
            setSearchResults(data);
          } else {
            setSearchResults([]); // Clear if term was cleared during fetch
          }
        }
      } catch (err: any) {
        if (mountedRef.current) {
          console.error("Search fetch error in hook:", err);
          setError(err.message || "Fejl ved søgning.");
          setSearchResults([]); // Clear results on error
        }
      } finally {
        if (mountedRef.current) {
          setIsLoading(false);
        }
      }
    }, debounceMs);

    // Cleanup function for this effect instance
    return () => {
      if (debounceTimeoutRef.current) {
        clearTimeout(debounceTimeoutRef.current);
      }
    };
  }, [searchTerm, debounceMs]); // Rerun effect if searchTerm or debounceMs changes

  // Function exposed to the component to trigger a search
  const search = useCallback((term: string) => {
    setSearchTerm(term);
    // Reset selected state if needed (component using the hook should handle this)
  }, []);

  // Function to manually clear results
  const clearResults = useCallback(() => {
    setSearchTerm(""); // Also clear the search term that drives the effect
    setSearchResults([]);
    setIsLoading(false);
    setError(null);
    if (debounceTimeoutRef.current) {
      clearTimeout(debounceTimeoutRef.current); // Clear any pending search
    }
  }, []);

  return { searchResults, isLoading, error, search, clearResults };
}
