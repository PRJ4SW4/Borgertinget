// src/components/Polidle/GuessList/GuessList.tsx
import React from "react";
import GuessItem from "../GuessItem/GuessItem";
import styles from "./GuessList.module.css"; // <<< BRUG CSS MODULE

import { GuessResultDto } from "../../../types/PolidleTypes";

interface GuessListProps {
  results: GuessResultDto[];
}

const GuessList: React.FC<GuessListProps> = ({ results }) => {
  if (!results || results.length === 0) {
    return <div className={styles.guessListEmpty}>Lav dit første gæt...</div>;
  }

  const headers = [
    "Politiker",
    "Køn",
    "Parti",
    "Alder",
    "Region",
    "Uddannelse",
  ];

  return (
    <div className={styles.guessListContainer}>
      <div className={styles.guessListHeader}>
        {headers.map((headerText) => (
          <div className={styles.categoryHeader} key={headerText}>
            {headerText}
          </div>
        ))}
      </div>
      <div className={styles.guessListItems}>
        {results.map((result, index) => {
          const key = result.guessedPolitician
            ? `${result.guessedPolitician.id}-${index}`
            : `guess-${index}`;
          return result.guessedPolitician ? (
            <GuessItem key={key} result={result} />
          ) : (
            <div key={`error-${index}`} className={styles.guessItemError}>
              Fejl: Gætdata mangler.
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default GuessList;
