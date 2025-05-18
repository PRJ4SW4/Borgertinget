// src/components/Polidle/GuessList/GuessItem.tsx
import React from "react";
import styles from "./GuessItem.module.css";
import {
  GuessResultDto,
  DailyPoliticianDto,
  FeedbackType,
} from "../../../types/PolidleTypes";
import {
  FeedbackFieldKeys,
  getFeedbackClass,
} from "../../../hooks/GuessItem.logic";

interface GuessItemProps {
  result: GuessResultDto;
}

const GuessItem: React.FC<GuessItemProps> = ({ result }) => {
  if (!result.guessedPolitician) {
    console.error("GuessItem modtog et result uden guessedPolitician:", result);
    return (
      <div className={`${styles.guessItem} ${styles.errorRow}`}>
        Fejl: Manglende politiker data for dette gæt.
      </div>
    );
  }

  const guessed: DailyPoliticianDto = result.guessedPolitician;
  const feedback = result.feedback;

  const renderAgeFeedback = () => {
    if (!result.isCorrectGuess) {
      if (feedback[FeedbackFieldKeys.AGE] === FeedbackType.Lavere) {
        return <span className={styles.arrow}>&#8595;</span>;
      }
      if (feedback[FeedbackFieldKeys.AGE] === FeedbackType.Højere) {
        return <span className={styles.arrow}>&#8593;</span>;
      }
    }
    return null;
  };

  const renderPartyIndicator = () => {
    if (guessed.partyShortname) {
      return <span className={styles.partyLogo}>{guessed.partyShortname}</span>;
    }
    return (
      <span className={styles.valueText}>{guessed.partyShortname || "-"}</span>
    );
  };

  return (
    <div
      className={`${styles.guessItem} ${
        result.isCorrectGuess ? styles.guessCorrectOverall : ""
      }`}
    >
      <div
        className={`${styles.guessData} ${
          styles.politicianCell
        } ${getFeedbackClass(
          feedback[FeedbackFieldKeys.NAME],
          result.isCorrectGuess
        )}`}
      >
        {guessed.pictureUrl ? (
          <img
            src={guessed.pictureUrl}
            alt={guessed.politikerNavn}
            className={styles.politicianImage}
          />
        ) : (
          <div className={styles.politicianImagePlaceholder}>?</div>
        )}
        <div className={styles.valueText}>{guessed.politikerNavn || "N/A"}</div>
      </div>

      <div
        className={`${styles.guessData} ${getFeedbackClass(
          feedback[FeedbackFieldKeys.GENDER],
          result.isCorrectGuess
        )}`}
      >
        <span style={{ fontSize: "1.2em", fontWeight: "bold" }}>
          {guessed.køn || "-"}
        </span>
      </div>

      <div
        className={`${styles.guessData} ${getFeedbackClass(
          feedback[FeedbackFieldKeys.PARTYSHORTNAME],
          result.isCorrectGuess
        )}`}
      >
        {renderPartyIndicator()}
      </div>

      <div
        className={`${styles.guessData} ${getFeedbackClass(
          feedback[FeedbackFieldKeys.AGE],
          result.isCorrectGuess
        )}`}
      >
        <span style={{ fontSize: "1.2em", fontWeight: "bold" }}>
          {guessed.age}
        </span>
        {renderAgeFeedback()}
      </div>

      <div
        className={`${styles.guessData} ${getFeedbackClass(
          feedback[FeedbackFieldKeys.REGION],
          result.isCorrectGuess
        )}`}
      >
        <span style={{ fontSize: "0.9em" }}>{guessed.region || "-"}</span>
      </div>

      <div
        className={`${styles.guessData} ${getFeedbackClass(
          feedback[FeedbackFieldKeys.EDUCATION],
          result.isCorrectGuess
        )}`}
      >
        <span style={{ fontSize: "0.9em" }}>{guessed.uddannelse || "-"}</span>
      </div>
    </div>
  );
};

export default GuessItem;
