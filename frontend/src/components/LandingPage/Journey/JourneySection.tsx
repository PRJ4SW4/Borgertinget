// src/components/LandingPage/Journey/JourneySection.tsx
import React from "react";
import { useNavigate } from "react-router-dom";
import styles from "./JourneySection.module.css";
import Button from "../../common/Button";

interface JourneySectionProps {
  id?: string;
}

const JourneySection: React.FC<JourneySectionProps> = ({ id }) => {
  const navigate = useNavigate();

  return (
    <section id={id} className={styles.journeySection}>
      <div className={styles.container}>
        <h1>Start din rejse her</h1>
        <div className={styles.buttons}>
          <Button onClick={() => navigate("/login")}>
            Log Ind / Opret Bruger
          </Button>
        </div>
      </div>
    </section>
  );
};

export default JourneySection;
