// src/components/LandingPage/Mission/MissionSection.tsx
import React from "react";
import styles from "./MissionSection.module.css";
import missionImage from "../../../assets/denmark_map.png"; // Juster stien til dit billede
// Import af eventuelle andre komponenter

interface MissionSectionProps {
  id?: string;
  missionText: string; // Prop til at modtage din missionstekst
}

const MissionSection: React.FC<MissionSectionProps> = ({ id, missionText }) => {
  return (
    <section id={id} className={styles.missionSection}>
      <div className={styles.imageContainer}>
        <img
          src={missionImage}
          alt="Kort over Danmark"
          className={styles.image}
        />
      </div>
      <div className={styles.content}>
        <h1>Vores mission</h1>
        <p>{missionText}</p>
      </div>
    </section>
  );
};

export default MissionSection;
