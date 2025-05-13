// src/components/Polidle/GuessList/GuessItem.tsx
import React from "react";
import "./GuessItem.module.css";
// Importer centrale typer
import {
  GuessResultDto,
  FeedbackType,
  DailyPoliticianDto, // <<< OPDATERET: Bruger nu den centrale DailyPoliticianDto
} from "../../../types/PolidleTypes"; // << VIGTIGT: Sørg for korrekt sti

interface GuessItemProps {
  result: GuessResultDto;
}

// Definér nøglerne til feedback-objektet for at undgå magiske strenge
// Disse bør matche de nøgler, din backend bruger i GuessResultDto.feedback
// Du kan også overveje at definere dem i PolidleTypes.ts, hvis de er en fast del af kontrakten.
const FeedbackFieldKeys = {
  NAME: "Navn", // Eller hvad end din backend sender
  GENDER: "Køn",
  PARTY: "Parti",
  AGE: "Alder",
  REGION: "Region",
  EDUCATION: "Uddannelse",
} as const; // 'as const' gør værdierne til literal types for bedre type-sikkerhed

// Helper funktion til at mappe FeedbackType til CSS klasse
const getFeedbackClass = (feedbackType: FeedbackType | undefined): string => {
  if (feedbackType === undefined) return "unknown-feedback"; // En default klasse hvis feedback mangler
  switch (feedbackType) {
    case FeedbackType.Korrekt:
      return "correct";
    case FeedbackType.Forkert:
      return "incorrect";
    case FeedbackType.Højere:
      return "higher"; // CSS skal håndtere, hvordan 'higher' og 'lower' ser ud
    case FeedbackType.Lavere:
      return "lower";
    case FeedbackType.Undefined: // Hvis backend eksplicit sender Undefined
    default:
      return "unknown-feedback";
  }
};

const GuessItem: React.FC<GuessItemProps> = ({ result }) => {
  // Vi forventer nu at guessedPolitician altid er der, hvis GuessResultDto er valid.
  // Hvis den *kan* være null, skal det håndteres mere elegant end blot en fejlbesked,
  // eller også skal typen i GuessResultDto være DailyPoliticianDto (ikke nullable).
  // For nu antager vi, at den er der, baseret på vores forenklede GuessResultDto.
  if (!result.guessedPolitician) {
    console.error("GuessItem modtog et result uden guessedPolitician:", result);
    return (
      <div className="guess-item error-row">
        {" "}
        {/* Tilføj evt. specifik fejl-styling */}
        Fejl: Manglende politiker data for dette gæt.
      </div>
    );
  }

  // Nu er 'guessed' af typen DailyPoliticianDto
  const guessed: DailyPoliticianDto = result.guessedPolitician;
  const feedback = result.feedback;

  return (
    <div
      className={`guess-item ${
        result.isCorrectGuess ? "guess-correct-overall" : "" // CSS klasse for hvis hele gættet var rigtigt
      }`}
    >
      {/* Politiker Navn - Antager backend sender feedback for Navn hvis hele gættet ikke var korrekt */}
      <div
        className={`guess-data ${getFeedbackClass(
          feedback[FeedbackFieldKeys.NAME]
        )}`}
      >
        {guessed.politikerNavn || "N/A"} {/* Vis N/A hvis navn mangler */}
      </div>

      {/* Køn */}
      <div
        className={`guess-data ${getFeedbackClass(
          feedback[FeedbackFieldKeys.GENDER]
        )}`}
      >
        {guessed.køn || "-"} {/* Vis - hvis data mangler */}
      </div>

      {/* Parti - DailyPoliticianDto har 'parti' og evt. 'partyShortname' */}
      <div
        className={`guess-data ${getFeedbackClass(
          feedback[FeedbackFieldKeys.PARTY]
        )}`}
      >
        {guessed.parti || "-"} {/* Vis det fulde partinavn */}
      </div>

      {/* Alder */}
      <div
        className={`guess-data ${getFeedbackClass(
          feedback[FeedbackFieldKeys.AGE]
        )}`}
      >
        {guessed.age}
        {feedback[FeedbackFieldKeys.AGE] === FeedbackType.Lavere && (
          <span className="arrow"> &#8595;</span> /* Pil ned */
        )}
        {feedback[FeedbackFieldKeys.AGE] === FeedbackType.Højere && (
          <span className="arrow"> &#8593;</span> /* Pil op */
        )}
      </div>

      {/* Region */}
      <div
        className={`guess-data ${getFeedbackClass(
          feedback[FeedbackFieldKeys.REGION]
        )}`}
      >
        {guessed.region || "-"}
      </div>

      {/* Uddannelse */}
      <div
        className={`guess-data ${getFeedbackClass(
          feedback[FeedbackFieldKeys.EDUCATION]
        )}`}
      >
        {guessed.uddannelse || "-"}
      </div>
    </div>
  );
};

export default GuessItem;
