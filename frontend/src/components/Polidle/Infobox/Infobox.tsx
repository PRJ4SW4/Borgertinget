// src/components/Polidle/Infobox/Infobox.tsx
import React from "react";
import "./Infobox.module.css";
import { FeedbackType } from "../../../types/PolidleTypes"; // Importer FeedbackType

interface LegendInfo {
  id: string; // Bruges til React key og evt. specifik styling, hvis nødvendigt
  label: string;
  indicatorClassName: string; // CSS klasse for indikatoren
  feedbackType?: FeedbackType; // Valgfri, for semantisk kobling
}

// Definerer de items, der skal vises i infoboksen.
// Kobler label og CSS klasse til en FeedbackType for klarhed.
const legendItems: LegendInfo[] = [
  {
    id: "korrekt",
    label: "Korrekt",
    indicatorClassName: "korrekt", // Matcher din Infobox.css
    feedbackType: FeedbackType.Korrekt,
  },
  {
    id: "forkert",
    label: "Forkert",
    indicatorClassName: "forkert", // Matcher din Infobox.css
    feedbackType: FeedbackType.Forkert,
  },
  {
    id: "hoejere",
    label: "Højere", // Vises for Alder feedback
    indicatorClassName: "hoejere", // Matcher din Infobox.css (med pil)
    feedbackType: FeedbackType.Højere,
  },
  {
    id: "lavere",
    label: "Lavere", // Vises for Alder feedback
    indicatorClassName: "lavere", // Matcher din Infobox.css (med pil)
    feedbackType: FeedbackType.Lavere,
  },
  // FeedbackType.Undefined vises typisk ikke i legenden.
];

const Infobox: React.FC = () => {
  return (
    <div className="infobox">
      {legendItems.map((item) => (
        <div className="item" key={item.id}>
          {/* Indikator-div'en får CSS klassen fra item.indicatorClassName */}
          <div className={`indikator ${item.indicatorClassName}`}></div>
          <div className="label">{item.label}</div>
        </div>
      ))}
    </div>
  );
};

export default Infobox;
