// src/components/LandingPage/First/FirstSection.tsx
import React from "react";
import styles from "./FirstSection.module.css";
import firstSectionImage from "../../../assets/first_section_image.png"; // Juster stien til dit eksempelbillede
// TODO: Log ind knap component her >>>> import Button from "../../Common/Button";
import Button from "../../common/Button";

interface FirstSectionProps {
  id?: string;
}

const FirstSection: React.FC<FirstSectionProps> = ({ id }) => {
  return (
    <section id={id} className={styles.firstSection}>
      <div className={styles.content}>
        <h1>For et bedre samfund</h1>
        <p>Danmarks førende politiske læringsplatform</p>
        <Button onClick={() => console.log("Log på klikket")}>Log på</Button>
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
