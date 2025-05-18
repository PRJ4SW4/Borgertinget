// src/components/Polidle/GuessList/GuessItem.logic.ts
import { FeedbackType } from "../../../types/PolidleTypes";
import styles from "../components/Polidle/GuessItem/GuessItem.module.css";

export const FeedbackFieldKeys = {
  NAME: "Navn",
  GENDER: "Køn",
  PARTYSHORTNAME: "PartyShortname",
  AGE: "Alder",
  REGION: "Region",
  EDUCATION: "Uddannelse",
} as const;

export const getFeedbackClass = (
  feedbackType: FeedbackType | undefined,
  isOverallCorrect: boolean
): string => {
  if (isOverallCorrect) return styles.correct;

  if (feedbackType === undefined) return styles.unknownFeedback;

  switch (feedbackType) {
    case FeedbackType.Korrekt:
      return styles.correct;
    case FeedbackType.Forkert:
      return styles.incorrect;
    case FeedbackType.Højere:
      return styles.higher;
    case FeedbackType.Lavere:
      return styles.lower;
    case FeedbackType.Undefined:
    default:
      return styles.unknownFeedback;
  }
};
