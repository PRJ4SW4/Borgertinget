// src/components/Polidle/GuessList/GuessItem.tsx
import React from "react";
// VIGTIGT: Sørg for at importere styles som et CSS Module objekt
import styles from "./GuessItem.module.css";
import {
  GuessResultDto,
  FeedbackType,
  DailyPoliticianDto,
} from "../../../types/PolidleTypes"; // Sørg for korrekt sti

interface GuessItemProps {
  result: GuessResultDto;
}

const FeedbackFieldKeys = {
  NAME: "Navn",
  GENDER: "Køn",
  PARTYSHORTNAME: "PartyShortname",
  AGE: "Alder",
  REGION: "Region",
  EDUCATION: "Uddannelse",
} as const;

// Helper funktion til at mappe FeedbackType til CSS klasse
// Tager nu isOverallCorrect som et argument
const getFeedbackClass = (
  feedbackType: FeedbackType | undefined,
  isOverallCorrect: boolean // <<< NY PARAMETER
): string => {
  // Hvis hele gættet er korrekt, skal alle celler have 'correct' klassen
  if (isOverallCorrect) {
    return styles.correct; // <<< BRUGER styles.correct
  }

  // Ellers, brug den specifikke feedback type
  if (feedbackType === undefined) return styles.unknownFeedback; // Brug styles.unknownFeedback
  switch (feedbackType) {
    case FeedbackType.Korrekt:
      return styles.correct; // Brug styles.correct
    case FeedbackType.Forkert:
      return styles.incorrect; // Brug styles.incorrect
    case FeedbackType.Højere:
      return styles.higher; // Brug styles.higher
    case FeedbackType.Lavere:
      return styles.lower; // Brug styles.lower
    case FeedbackType.Undefined: // Hvis backend eksplicit sender Undefined
    default:
      return styles.unknownFeedback; // Brug styles.unknownFeedback
  }
};

const GuessItem: React.FC<GuessItemProps> = ({ result }) => {
  if (!result.guessedPolitician) {
    console.error("GuessItem modtog et result uden guessedPolitician:", result);
    // Sørg for at errorRow er defineret i dit GuessItem.module.css eller en global stilfil
    return (
      <div className={`${styles.guessItem} ${styles.errorRow}`}>
        Fejl: Manglende politiker data for dette gæt.
      </div>
    );
  }

  const guessed: DailyPoliticianDto = result.guessedPolitician;
  const feedback = result.feedback;

  const renderAgeFeedback = () => {
    // Vis kun pile hvis det overordnede gæt IKKE er korrekt
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
      //*FIXED .parti should be changed to partyShortname
      return <span className={styles.partyLogo}>{guessed.partyShortname}</span>; //*FIXED .parti should be changed to partyShortname
    }
    return (
      <span className={styles.valueText}>{guessed.partyShortname || "-"}</span>
    );
  };

  return (
    <div
      className={`${styles.guessItem} ${
        // Base klasse
        result.isCorrectGuess ? styles.guessCorrectOverall : "" // Gylden kant hvis hele gættet er korrekt
      }`}
    >
      {/* Politiker Celle */}
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

      {/* Køn Celle */}
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

      {/* Parti Celle */}
      <div
        className={`${styles.guessData} ${getFeedbackClass(
          feedback[FeedbackFieldKeys.PARTYSHORTNAME],
          result.isCorrectGuess
        )}`}
      >
        {renderPartyIndicator()}
      </div>

      {/* Alder Celle */}
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

      {/* Region Celle */}
      <div
        className={`${styles.guessData} ${getFeedbackClass(
          feedback[FeedbackFieldKeys.REGION],
          result.isCorrectGuess
        )}`}
      >
        <span style={{ fontSize: "0.9em" }}>{guessed.region || "-"}</span>
      </div>

      {/* Uddannelse Celle */}
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
