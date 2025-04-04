import React from "react";
import GuessItem from "./GuessItem";
import "./GuessList.css";

interface Guess {
  politikker: string;
  køn: string;
  parti: string;
  alder: number;
  region: string;
  uddannelse: string;
}

interface GuessListProps {
  guesses: Guess[];
  correctAnswers: Guess;
}

const GuessList: React.FC<GuessListProps> = ({ guesses, correctAnswers }) => {
  return (
    <div className="guess-list">
      <div className="header">
        <div className="category">Politiker</div>
        <div className="category">Køn</div>
        <div className="category">Parti</div>
        <div className="category">Alder</div>
        <div className="category">Region</div>
        <div className="category">Uddannelse</div>
      </div>
      {guesses.map((guess, index) => (
        <GuessItem
          key={index}
          guess={guess}
          isCorrect={{
            politikker: guess.politikker === correctAnswers.politikker,
            køn: guess.køn === correctAnswers.køn,
            parti: guess.parti === correctAnswers.parti,
            alder: guess.alder === correctAnswers.alder,
            region: guess.region === correctAnswers.region,
            uddannelse: guess.uddannelse === correctAnswers.uddannelse,
          }}
          ageDifference={guess.alder - correctAnswers.alder}
        />
      ))}
    </div>
  );
};

export default GuessList;
