import React from "react";
import "./GuessItem.css";

interface Guess {
  politikker: string;
  køn: string;
  parti: string;
  alder: number;
  region: string;
  uddannelse: string;
}

interface GuessItemProps {
  guess: Guess;
  isCorrect: {
    politikker: boolean;
    køn: boolean;
    parti: boolean;
    alder: boolean;
    region: boolean;
    uddannelse: boolean;
  };
  ageDifference: number;
}

const GuessItem: React.FC<GuessItemProps> = ({
  guess,
  isCorrect,
  ageDifference,
}) => {
  return (
    <div className="guess-item">
      <div
        className={`guess-data ${
          isCorrect.politikker ? "correct" : "incorrect"
        }`}
      >
        {guess.politikker}
      </div>
      <div className={`guess-data ${isCorrect.køn ? "correct" : "incorrect"}`}>
        {guess.køn}
      </div>
      <div
        className={`guess-data ${isCorrect.parti ? "correct" : "incorrect"}`}
      >
        {guess.parti}
      </div>
      <div
        className={`guess-data ${isCorrect.alder ? "correct" : "incorrect"}`}
      >
        {guess.alder}
        {!isCorrect.alder && ageDifference > 0 && (
          <span className="arrow">&#8595;</span>
        )}
        {!isCorrect.alder && ageDifference < 0 && (
          <span className="arrow">&#8593;</span>
        )}
      </div>
      <div
        className={`guess-data ${isCorrect.region ? "correct" : "incorrect"}`}
      >
        {guess.region}
      </div>
      <div
        className={`guess-data ${
          isCorrect.uddannelse ? "correct" : "incorrect"
        }`}
      >
        {guess.uddannelse}
      </div>
    </div>
  );
};

export default GuessItem;
