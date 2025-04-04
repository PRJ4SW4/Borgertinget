// src/pages/LandingPage/LandingPage.tsx
import React, { useEffect } from "react";
import HeroSection from "../../components/LandingPage/Hero/HeroSection";
import FirstSection from "../../components/LandingPage/First/FirstSection";
import MissionSection from "../../components/LandingPage/Mission/MissionSection";
import FeaturesSection from "../../components/LandingPage/Features/FeaturesSection";
import TeamSection from "../../components/LandingPage/Team/TeamSection";
import JourneySection from "../../components/LandingPage/Journey/JourneySection";
import Footer from "../../components/LandingPage/Footer/Footer";
import styles from "./LandingPage.module.css";

interface LandingPageProps {}

const LandingPage: React.FC<LandingPageProps> = () => {
  const missionTekst = `
  Indsæt tekst som skal placeres på MissionTekst component
`;

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

    const anchorLinks =
      document.querySelectorAll<HTMLAnchorElement>('a[href^="#"]');
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
            <a href="#first">Introduktion</a>
          </li>
          <li>
            <a href="#mission">Mission</a>
          </li>
          <li>
            <a href="#features1">Funktioner</a>
          </li>{" "}
          {/* Tilføj unikke ID'er */}
          <li>
            <a href="#team">Team</a>
          </li>
          <li>
            <a href="#features2">Flere Funktioner</a>
          </li>{" "}
          {/* Tilføj unikke ID'er */}
          <li>
            <a href="#journey">Rejse</a>
          </li>
          <li>
            <a href="#contact">Kontakt</a>
          </li>{" "}
          {/* Tilføj kontaktsektion hvis du har en */}
        </ul>
      </nav>
      <HeroSection id="hero" /> {/* Tilføj ID'er til sektionerne */}
      <FirstSection id="first" />
      <MissionSection id="mission" missionText={/* Din missionstekst */} />
      <FeaturesSection id="features1" /> {/* Brug unikke ID'er */}
      <TeamSection id="team" />
      <FeaturesSection id="features2" /> {/* Brug unikke ID'er */}
      <JourneySection id="journey" />
      <Footer />
    </div>
  );
};

export default LandingPage;
