// src/components/LandingPage/Footer/Footer.tsx
import React from "react";
import styles from "./Footer.module.css";
import logo from "../../../assets/borgertinget_logo_small.png"; // Juster stien til dit logo

interface FooterProps {}

const Footer: React.FC<FooterProps> = () => {
  return (
    <footer className={styles.footer}>
      <div className={styles.logoContainer}>
        <img src={logo} alt="Borgertinget Logo" className={styles.logo} />
      </div>
      <div className={styles.placeholderText}>
        <p>
          &copy; {new Date().getFullYear()} Borgertinget. Alle rettigheder
          forbeholdes.
        </p>
        <p>Noget mere placeholder tekst her.</p>
        {/* Du kan tilføje flere placeholder elementer efter behov */}
      </div>
    </footer>
  );
};

export default Footer;
