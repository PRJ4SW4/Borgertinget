// src/components/Polidle/Infobox/Infobox.tsx
import React from "react";
import styles from "./Infobox.module.css"; // <<< Brug CSS Module import
import { FeedbackType } from "../../../types/PolidleTypes";

interface LegendInfo {
  id: string;
  label: string;
  indicatorClassName: string; // Bruges til at style firkanten
  icon?: string; // Valgfri for pil-symboler
  feedbackType?: FeedbackType;
}

// Opdateret med 'icon' for pile
const legendItems: LegendInfo[] = [
  {
    id: "korrekt",
    label: "Korrekt",
    indicatorClassName: styles.korrekt,
    feedbackType: FeedbackType.Korrekt,
  },
  {
    id: "forkert",
    label: "Forkert",
    indicatorClassName: styles.forkert,
    feedbackType: FeedbackType.Forkert,
  },
  {
    id: "hoejere",
    label: "Højere",
    indicatorClassName: styles.hoejere,
    icon: "↑",
    feedbackType: FeedbackType.Højere,
  },
  {
    id: "lavere",
    label: "Lavere",
    indicatorClassName: styles.lavere,
    icon: "↓",
    feedbackType: FeedbackType.Lavere,
  },
];

interface InfoboxProps {
  title?: string; // Gør titlen valgfri
}

const Infobox: React.FC<InfoboxProps> = ({ title = "Infoboks" }) => {
  // Default titel
  return (
    <div className={styles.infoboxContainer}>
      {" "}
      {/* Ydre container for centrering/bredde */}
      <h3 className={styles.infoboxTitle}>{title}</h3>
      <div className={styles.infobox}>
        {" "}
        {/* Selve boksen med grå baggrund */}
        {legendItems.map((item) => (
          <div className={styles.item} key={item.id}>
            <div className={`${styles.indikator} ${item.indicatorClassName}`}>{item.icon && <span className={styles.icon}>{item.icon}</span>}</div>
            <div className={styles.label}>{item.label}</div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default Infobox;
