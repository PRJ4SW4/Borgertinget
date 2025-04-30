// components/Polidle/GuessItem/GuessItem.tsx
import React from "react";
import "./GuessItem.css"; // Behold eller opdater din CSS
// Importer typerne
import {
  GuessResultDto,
  FeedbackType,
  GuessedPoliticianDetailsDto,
} from "../../../pages/Polidle/ClassicMode"; // Juster sti

interface GuessItemProps {
  result: GuessResultDto; // Modtager nu ét resultat objekt
}

// Helper funktion til at mappe FeedbackType til CSS klasse
const getFeedbackClass = (feedbackType: FeedbackType | undefined): string => {
  if (feedbackType === undefined) return "";
  switch (feedbackType) {
    case FeedbackType.Korrekt:
      return "correct";
    case FeedbackType.Forkert:
      return "incorrect";
    case FeedbackType.Højere:
      return "higher";
    case FeedbackType.Lavere:
      return "lower";
    default:
      return "";
  }
};

const GuessItem: React.FC<GuessItemProps> = ({ result }) => {
  if (!result.guessedPolitician) {
    return (
      <div className="guess-item error">
        Fejl: Manglende politiker data for dette gæt.
      </div>
    );
  }

  const guessed = result.guessedPolitician;
  const feedback = result.feedback;

  return (
    <div
      className={`guess-item ${
        result.isCorrectGuess ? "guess-correct-overall" : ""
      }`}
    >
      {/* Politiker Navn */}
      <div className={`guess-data ${getFeedbackClass(feedback["Navn"])}`}>
        {guessed.politikerNavn}
      </div>
      {/* Køn */}
      <div className={`guess-data ${getFeedbackClass(feedback["Køn"])}`}>
        {guessed.køn}
      </div>
      {/* Parti */}
      <div className={`guess-data ${getFeedbackClass(feedback["Parti"])}`}>
        {guessed.partiNavn}
      </div>
      {/* Alder */}
      <div className={`guess-data ${getFeedbackClass(feedback["Alder"])}`}>
        {/* VIS SELVE ALDEREN */}
        {guessed.age}
        {/* Vis pil baseret på Højere/Lavere feedback */}
        {feedback["Alder"] === FeedbackType.Lavere && (
          <span className="arrow"> &#8595;</span>
        )}
        {feedback["Alder"] === FeedbackType.Højere && (
          <span className="arrow"> &#8593;</span>
        )}
      </div>
      {/* Region */}
      <div className={`guess-data ${getFeedbackClass(feedback["Region"])}`}>
        {guessed.region}
      </div>
      {/* Uddannelse */}
      <div className={`guess-data ${getFeedbackClass(feedback["Uddannelse"])}`}>
        {guessed.uddannelse}
      </div>
    </div>
  );
};

export default GuessItem;
