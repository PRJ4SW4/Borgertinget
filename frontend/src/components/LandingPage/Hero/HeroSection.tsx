// src/components/LandingPage/Hero/HeroSection.tsx
import React from "react";
import styles from "./HeroSection.module.css"; // Vi opretter denne CSS-modul fil senere
import heroImage from "../../../images/LoginImage.png"; // Juster stien til dit billede

interface HeroSectionProps {
  id?: string;
}

const HeroSection: React.FC<HeroSectionProps> = ({ id }) => {
  return (
    <section id={id} className={styles.hero}>
      <img
        src={heroImage}
        alt="Borgertinget Logo"
        className={styles.heroImage}
      />
    </section>
  );
};

export default HeroSection;
