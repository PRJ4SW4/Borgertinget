// src/components/LandingPage/LandingPage.tsx
import React, { useEffect } from "react";
// ... import af andre komponenter ...
import styles from "./LandingPage.module.css";

const LandingPage: React.FC = () => {
  useEffect(() => {
    const smoothScroll = (targetId: string) => {
      const targetElement = document.getElementById(targetId);
      if (targetElement) {
        window.scrollTo({
          top: targetElement.offsetTop,
          behavior: "smooth",
        });
      }
    };

    const handleAnchorClick = (
      e: React.MouseEvent<HTMLAnchorElement, MouseEvent>
    ) => {
      e.preventDefault();
      const targetId = e.currentTarget.getAttribute("href")?.substring(1);
      if (targetId) {
        smoothScroll(targetId);
      }
    };

    const anchorLinks = document.querySelectorAll('a[href^="#"]');
    anchorLinks.forEach((link) => {
      link.addEventListener("click", handleAnchorClick);
    });

    return () => {
      anchorLinks.forEach((link) => {
        link.removeEventListener("click", handleAnchorClick);
      });
    };
  }, []);

  return (
    <div className={styles.landingPage}>
      <nav>
        <ul>
          <li>
            <a href="#hero">Start</a>
          </li>
          <li>
            <a href="#purpose">Formål</a>
          </li>
          <li>
            <a href="#features">Funktioner</a>
          </li>
          <li>
            <a href="#team">Team</a>
          </li>
          <li>
            <a href="#contact">Kontakt</a>
          </li>
        </ul>
      </nav>
      <HeroSection />
      <PurposeSection id="purpose" />
      <FeaturesSection id="features" />
      <TeamSection id="team" />
      <ContactSection id="contact" />
      <Footer />
    </div>
  );
};

export default LandingPage;
