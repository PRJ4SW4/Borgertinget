// src/components/LandingPage/PurposeSection.tsx
import React from "react";
import styles from "./MissionSection.module.css";

interface MissionSectionProps {}

const MissionSection: React.FC<MissionSectionProps> = () => {
  return (
    <section id="purpose" className={styles.purpose}>
      <h2>Formålet med Projektet</h2>
      <p>Uddyb her formålet og visionen bag dit React-projekt.</p>
      {/* ... mere indhold ... */}
    </section>
  );
};

export default MissionSection;
