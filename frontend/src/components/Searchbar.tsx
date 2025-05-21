import React, { useState, ChangeEvent, KeyboardEvent, useCallback, useEffect, useRef } from "react";
import { Link } from "react-router-dom";
import { SearchDocument } from "../types/searchResult";
import "./Searchbar.css"; // Ensure this CSS file exists and is styled

// Placeholder for SearchDocument type

const SearchBar: React.FC = () => {
  const [searchQuery, setSearchQuery] = useState<string>("");
  const [searchResults, setSearchResults] = useState<SearchDocument[]>([]);
  const [searchSuggestions, setSearchSuggestions] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [isSuggestionsLoading, setIsSuggestionsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [showSuggestions, setShowSuggestions] = useState<boolean>(false);
  const [isFullSearchActive, setIsFullSearchActive] = useState<boolean>(false);

  const SUGGEST_DEBOUNCE_DELAY = 200;

  const searchInputRef = useRef<HTMLInputElement>(null);
  const searchContainerRef = useRef<HTMLDivElement>(null);

  const executeSearch = useCallback(async (query: string) => {
    const trimmedQuery = query.trim();
    if (!trimmedQuery) {
      setSearchResults([]);
      setError(null);
      setIsLoading(false);
      setIsFullSearchActive(false); // Reset if query is empty
      return;
    }

    setIsFullSearchActive(true);
    setIsLoading(true);
    setError(null);
    setShowSuggestions(false);

    try {
      const encodedQuery = encodeURIComponent(trimmedQuery);
      const response = await fetch(`/api/Search?query=${encodedQuery}`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          authorization: `Bearer ${localStorage.getItem("jwt")}`,
        },
      });
      if (!response.ok) {
        const errorData = await response.text();
        throw new Error(`Search failed with status: ${response.status}. Message: ${errorData}`);
      }
      const data: SearchDocument[] = await response.json();
      setSearchResults(data);
    } catch (err) {
      console.error("Search error:", err);
      setError(err instanceof Error ? err.message : "An unknown error occurred during search.");
      setSearchResults([]);
    } finally {
      setIsLoading(false);
    }
  }, []);

  const fetchSuggestions = useCallback(
    async (query: string) => {
      if (isFullSearchActive) {
        // If a full search is active, don't fetch suggestions.
        setShowSuggestions(false); // Ensure suggestions are hidden.
        return;
      }

      const trimmedQuery = query.trim();
      if (!trimmedQuery) {
        setSearchSuggestions([]);
        setShowSuggestions(false);
        return;
      }
      setIsSuggestionsLoading(true);
      try {
        const encodedQuery = encodeURIComponent(trimmedQuery);
        const response = await fetch(`/api/Search/suggest?prefix=${encodedQuery}`, {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            authorization: `Bearer ${localStorage.getItem("jwt")}`,
          },
        });
        if (!response.ok) {
          console.error(`Suggestion fetch failed with status: ${response.status}`);
          setSearchSuggestions([]);
          setShowSuggestions(false);
          return;
        }
        const data: string[] = await response.json();
        setSearchSuggestions(data);
        // Only show suggestions if not in the middle of a full search and data exists
        if (!isFullSearchActive) {
          setShowSuggestions(data.length > 0);
        } else {
          setShowSuggestions(false); // Ensure hidden if full search became active during fetch
        }
      } catch (err) {
        console.error("Suggestion fetch error:", err);
        setSearchSuggestions([]);
        setShowSuggestions(false);
      } finally {
        setIsSuggestionsLoading(false);
      }
    },
    [isFullSearchActive]
  ); // isFullSearchActive is a key dependency

  useEffect(() => {
    const trimmedQuery = searchQuery.trim();
    if (!trimmedQuery) {
      setSearchSuggestions([]);
      setShowSuggestions(false);
      setIsSuggestionsLoading(false);
      if (searchQuery === "") {
        setSearchResults([]);
        setError(null);
      }
      setIsFullSearchActive(false); // Reset if query is empty
      return;
    }

    // If a full search is active (likely because searchQuery was just set by a suggestion click or Enter),
    // don't immediately re-fetch suggestions for the exact same term.
    // The isFullSearchActive flag will be reset by executeSearch's finally block.
    if (isFullSearchActive) {
      return;
    }

    const suggestionTimerId = setTimeout(() => {
      fetchSuggestions(trimmedQuery);
    }, SUGGEST_DEBOUNCE_DELAY);

    return () => {
      clearTimeout(suggestionTimerId);
    };
  }, [searchQuery, fetchSuggestions, isFullSearchActive]);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (searchContainerRef.current && !searchContainerRef.current.contains(event.target as Node)) {
        setShowSuggestions(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  const handleInputChange = (event: ChangeEvent<HTMLInputElement>) => {
    const newQuery = event.target.value;
    setIsFullSearchActive(false);
    setSearchQuery(newQuery);

    if (newQuery.trim()) {
      setShowSuggestions(true);
    } else {
      setSearchResults([]);
      setError(null);
      setSearchSuggestions([]);
      setShowSuggestions(false);
    }
  };

  const handleSuggestionClick = (suggestion: string) => {
    setIsFullSearchActive(true);
    setShowSuggestions(false);
    setSearchQuery(suggestion);
    executeSearch(suggestion);
    if (searchInputRef.current) {
      searchInputRef.current.focus();
    }
  };

  const handleSearchKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter") {
      const trimmedQuery = searchQuery.trim();
      if (trimmedQuery) {
        setIsFullSearchActive(true); // Signal that a full search is being initiated
        setShowSuggestions(false); // Hide suggestions immediately
        // setSearchQuery is already up-to-date
        executeSearch(trimmedQuery);
      }
    }
  };

  const onInputFocus = () => {
    // Only show suggestions on focus if there's a query, suggestions exist,
    // and we are not in the middle of a full search action that just completed.
    if (searchQuery.trim() && !isFullSearchActive && searchSuggestions.length > 0) {
      setShowSuggestions(true);
    }
  };

  const getResultLink = (result: SearchDocument): string => {
    const parts = result.id.split("-");
    const actualId = parts.length > 1 ? parts.slice(1).join("-") : result.id;
    switch (result.dataType.toLowerCase()) {
      case "aktor":
        return `/politician/${actualId}`;
      case "flashcard":
        return result.collectionId ? `/flashcards/${result.collectionId}#flashcard-${actualId}` : "/flashcards";
      case "party":
        return `/party/${encodeURIComponent(result.partyName || "unknown")}`;
      case "page":
        return `/learning/${actualId}`;
      default:
        return "#";
    }
  };

  const getDataTypeDisplayName = (dataType: string): string => {
    switch (dataType.toLowerCase()) {
      case "aktor":
        return "Politiker";
      case "flashcard":
        return "Flashcard";
      case "party":
        return "Parti";
      case "page":
        return "Læringsside";
      default:
        return dataType;
    }
  };

  const shouldShowSearchFeedback =
    isLoading || error || (!isLoading && !error && searchQuery.trim() && searchResults.length === 0 && !showSuggestions);

  return (
    <div className="search-component-container" ref={searchContainerRef}>
      <input
        ref={searchInputRef}
        type="search"
        placeholder="Søg på tværs af Borgertinget..."
        className="search-input"
        value={searchQuery}
        onChange={handleInputChange}
        onKeyDown={handleSearchKeyDown}
        onFocus={onInputFocus}
      />
      {showSuggestions && searchQuery.trim() && !isFullSearchActive && (
        <ul className="search-suggestions-list">
          {isSuggestionsLoading && <li className="suggestion-item loading">Henter forslag...</li>}
          {!isSuggestionsLoading &&
            searchSuggestions.length > 0 &&
            searchSuggestions.map((suggestion, index) => (
              <li key={index} className="suggestion-item" onMouseDown={() => handleSuggestionClick(suggestion)}>
                {suggestion}
              </li>
            ))}
          {!isSuggestionsLoading && searchSuggestions.length === 0 && <li className="suggestion-item no-suggestions">Ingen forslag fundet.</li>}
        </ul>
      )}

      {(shouldShowSearchFeedback || (searchResults.length > 0 && !isLoading && !error)) && (
        <div className="search-results-feedback-container">
          {isLoading && <p className="search-loading">Søger...</p>}
          {error && <p className="search-error">Fejl: {error}</p>}
          {!isLoading && !error && searchQuery.trim() && searchResults.length === 0 && !showSuggestions && (
            <p className="search-no-results">Ingen resultater fundet for "{searchQuery}".</p>
          )}
          {searchResults.length > 0 && !isLoading && !error && (
            <ul className="search-results-list main-results">
              {searchResults.map((result) => (
                <li key={result.id} className="search-result-item">
                  <Link
                    to={getResultLink(result)}
                    className="search-result-link"
                    onClick={() => {
                      setShowSuggestions(false);
                      setIsFullSearchActive(true); // Indicate that clicking a result is like a "full search" action
                    }}>
                    <span className={`result-type-badge type-${getDataTypeDisplayName(result.dataType).toLowerCase().replace(/\s+/g, "-")}`}>
                      {getDataTypeDisplayName(result.dataType)}
                    </span>
                    <span className="result-title">
                      {result.title ||
                        result.aktorName ||
                        result.collectionTitle ||
                        result.pageTitle ||
                        result.partyName ||
                        result.frontText ||
                        result.backText ||
                        "Ukendt Titel"}
                    </span>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  );
};

export default SearchBar;
