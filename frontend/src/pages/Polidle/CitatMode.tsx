import React, { useState } from "react";
import Input from "../../components/Polidle/Input/Input";
import styles from "./Polidle.module.css";
import GameSelector from "../../components/Polidle/GamemodeSelector/GamemodeSelector"; // Import GameSelector

interface CitatModeProps {
  citat: string;
  correctPolitiker: string;
  politikerImage: string;
}

interface GuessInput {
  politikker: string;
}

const CitatMode: React.FC<CitatModeProps> = ({
  citat,
  correctPolitiker,
  politikerImage,
}) => {
  const [guess, setGuess] = useState("");
  const [isCorrect, setIsCorrect] = useState<boolean | null>(null);

  const handleGuess = (guessInput: GuessInput) => {
    const guessName = guessInput.politikker.toLowerCase();
    const correctName = correctPolitiker.toLowerCase();
    const result = guessName === correctName;
    setIsCorrect(result);
    setGuess(guessInput.politikker);
  };

  return (
    <div className={styles.container}>
      <h1 className={styles.heading}>Polidle - Citat Mode</h1>
      <p className={styles.paragraph}>Hvem sagde dette citat?</p>
      <p className={styles.citat}>"{citat}"</p> //Todo: Skal kobles til DB, så
      {/* TODO: citat tages fra "Dagens Polidle" */}
      <GameSelector /> {/* Tilføj GameSelector */}
      <Input onGuess={handleGuess} />
      {isCorrect !== null && (
        <div style={{ display: "inline-block" }}>
          <div
            className={`guess-result ${isCorrect ? "correct" : "incorrect"}`}
          >
            <img
              src={politikerImage}
              alt={guess}
              className={styles.resultImage}
            />
            <span className={styles.resultName}>{guess}</span>
          </div>
        </div>
      )}
    </div>
  );
};

export default CitatMode;
