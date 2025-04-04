import React, { useState } from "react";
import Infobox from "../../components/Polidle/Infobox/Infobox";
import Input from "../../components/Polidle/Input/Input";
import GuessList from "../../components/Polidle/GuessList/GuessList";
import styles from "./Polidle.module.css";
import GameSelector from "../../components/Polidle/GamemodeSelector/GamemodeSelector"; // Import GameSelector

interface PoliticianGuess {
  politikker: string;
  køn: string;
  parti: string;
  alder: number;
  region: string;
  uddannelse: string;
}

const ClassicMode: React.FC = () => {
  const [guesses, setGuesses] = useState<PoliticianGuess[]>([
    {
      politikker: "Mette Frederiksen",
      køn: "Kvinde",
      parti: "Socialdemokratiet",
      alder: 48,
      region: "Hovedstaden",
      uddannelse: "Cand.jur",
    },
    {
      politikker: "Lars Løkke",
      køn: "Mand",
      parti: "Moderaterne",
      alder: 50,
      region: "Hovedstaden",
      uddannelse: "Cand.jur",
    },
    // ... flere gæt ...
  ]);

  const correctAnswers: PoliticianGuess = {
    politikker: "Mette Frederiksen",
    køn: "Kvinde",
    parti: "Socialdemokratiet",
    alder: 48,
    region: "Hovedstaden",
    uddannelse: "Cand.jur",
  };

  const handleGuess = (guess: PoliticianGuess) => {
    setGuesses([...guesses, guess]);
  };

  return (
    <div className={styles.container}>
      <h1 className={styles.heading}>Polidle</h1>
      <h2 className={styles.subheading}>Velkommen til Dagens Polidle!</h2>
      <p className={styles.paragraph}>Prøv og gætte dagens politiker</p>
      <GameSelector></GameSelector>
      <Input onGuess={handleGuess} />
      <GuessList guesses={guesses} correctAnswers={correctAnswers} />
      <h2 style={{ color: "red" }}>Infobox</h2>
      <Infobox />
    </div>
  );
};

export default ClassicMode;
