// src/components/Polidle/GuessList/GuessList.tsx
import React from "react";
import GuessItem from "../GuessItem/GuessItem"; // Sørg for at GuessItem også bruger opdaterede typer
import "./GuessList.module.css";

// Importer den centrale type
import { GuessResultDto } from "../../../types/PolidleTypes"; // <<< OPDATERET IMPORT

interface GuessListProps {
  results: GuessResultDto[];
}

const GuessList: React.FC<GuessListProps> = ({ results }) => {
  if (!results || results.length === 0) {
    return <div className="guess-list-empty">Lav dit første gæt...</div>;
  }

  // Definer header labels for nemmere vedligeholdelse og evt. senere internationalisering
  const headers = [
    "Politiker",
    "Køn",
    "Parti",
    "Alder",
    "Region",
    "Uddannelse",
  ];

  return (
    <div className="guess-list-container">
      {" "}
      {/* Omdøb evt. ydre div for klarhed ift. CSS */}
      <div className="guess-list-header">
        {" "}
        {/* Omdøb evt. header div for klarhed */}
        {headers.map((headerText) => (
          <div className="category-header" key={headerText}>
            {" "}
            {/* Omdøb evt. category div */}
            {headerText}
          </div>
        ))}
      </div>
      <div className="guess-list-items">
        {results.map((result, index) => {
          // Generer en mere robust key, hvis muligt og nødvendigt.
          // Her bruger vi en kombination, hvis guessedPolitician eksisterer.
          // Hvis gæt på samme politiker er tilladt flere gange, er index stadig nødvendig for unikhed.
          const key = result.guessedPolitician
            ? `${result.guessedPolitician.id}-${index}`
            : index;

          // Vi forventer at result.guessedPolitician er udfyldt fra backend,
          // baseret på vores opdaterede GuessResultDto
          // Hvis det *kan* være null/undefined selv i en gyldig situation, skal der være bedre fallback.
          return result.guessedPolitician ? (
            <GuessItem key={key} result={result} />
          ) : (
            // Dette bør ideelt set ikke ske. Log en fejl hvis det gør.
            <div key={`error-${index}`} className="guess-item-error">
              Fejl: Gætdata mangler.
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default GuessList;
