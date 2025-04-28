// components/Polidle/GuessList/GuessList.tsx
import React from "react";
import GuessItem from "./GuessItem";
import "./GuessList.css"; // Behold eller opdater din CSS
// Importer typerne fra ClassicMode eller en delt types fil
import { GuessResultDto } from "../../../pages/Polidle/ClassicMode"; // Juster stien efter behov

interface GuessListProps {
  results: GuessResultDto[]; // Modtager nu listen af resultater
}

const GuessList: React.FC<GuessListProps> = ({ results }) => {
  // Hvis der ingen gæt er, vis intet eller en besked
  if (!results || results.length === 0) {
    return <div className="guess-list-empty">Lav dit første gæt...</div>;
  }

  return (
    <div className="guess-list">
      {/* Header forbliver den samme */}
      <div className="header">
        <div className="category">Politiker</div>
        <div className="category">Køn</div>
        <div className="category">Parti</div>
        <div className="category">Alder</div>
        <div className="category">Region</div>
        <div className="category">Uddannelse</div>
      </div>
      {/* Map over resultaterne fra backend */}
      {results.map(
        (result, index) =>
          // Tjek om guessedPolitician findes før rendering
          result.guessedPolitician ? (
            <GuessItem
              key={index} // Overvej at bruge et mere unikt ID hvis muligt, f.eks. result.guessedPolitician.id hvis gæt ikke kan gentages
              result={result} // Send hele resultat-objektet til GuessItem
            />
          ) : null // Undlad at rendere hvis guessedPolitician mangler (bør ikke ske)
      )}
    </div>
  );
};

export default GuessList;
