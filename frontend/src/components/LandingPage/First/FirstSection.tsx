// src/components/LandingPage/First/FirstSection.tsx
import React from "react";
import styles from "./FirstSection.module.css";
import { useNavigate } from "react-router-dom";
import firstSectionImage from "../../../assets/first_section_image.png";
import Button from "../../common/Button"; // Sørg for at stien er korrekt
//TODO: Erstat Button med den aktuellle Log in knap

interface FirstSectionProps {
  id?: string;
}

const FirstSection: React.FC<FirstSectionProps> = ({ id }) => {
  const navigate = useNavigate();
  return (
    <section id={id} className={styles.firstSection}>
      <div className={styles.content}>
        {/* Tekstdel */}
        <div>
          {" "}
          {/* Valgfri wrapper for h1 og p, hvis du vil style dem samlet */}
          <h1>For et bedre samfund</h1>
          <p>Danmarks førende politiske læringsplatform</p>
        </div>
        {/* Knap med specifik klasse */}
        <Button className={styles.ctaButton} onClick={() => navigate("/login")}>
            Log Ind / Opret Bruger
        </Button>
      </div>
      <div className={styles.imageContainer}>
        <img
          src={firstSectionImage}
          alt="Eksempelbillede"
          className={styles.image}
        />
      </div>
    </section>
  );
};

export default FirstSection;
