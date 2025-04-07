// src/components/LandingPage/Journey/JourneySection.tsx
import React from "react";
import styles from "./JourneySection.module.css";
import Button from "../../common/Button"; // Antager din genbrugelige knapkomponent

interface JourneySectionProps {
  id?: string;
}

const JourneySection: React.FC<JourneySectionProps> = ({ id }) => {
  return (
    <section id={id} className={styles.journeySection}>
      <div className={styles.container}>
        <h1>Start din rejse her</h1>
        <div className={styles.buttons}>
          <Button onClick={() => console.log("Log ind klikket")}>
            Log ind
          </Button>
          <Button onClick={() => console.log("Opret bruger klikket")}>
            Opret bruger
          </Button>
        </div>
      </div>
    </section>
  );
};

export default JourneySection;
