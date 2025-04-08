// src/pages/LandingPage/LandingPage.tsx
import React, { useEffect, useRef } from "react";
import HeroSection from "../../components/LandingPage/Hero/HeroSection";
import FirstSection from "../../components/LandingPage/First/FirstSection";
import MissionSection from "../../components/LandingPage/Mission/MissionSection";
import FeaturesSection from "../../components/LandingPage/Features/FeaturesSection";
import TeamSection from "../../components/LandingPage/Team/TeamSection";
import JourneySection from "../../components/LandingPage/Journey/JourneySection";
import Footer from "../../components/LandingPage/Footer/Footer";
import styles from "./LandingPage.module.css";

interface FeatureItem {
  title: string;
  image: string;
  link: string;
}

interface LandingPageProps {}

const LandingPage: React.FC<LandingPageProps> = () => {
  const missionTekst = `
    "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor 
    incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud 
    exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute 
    irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla 
    pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia 
    deserunt mollit anim id est laborum."
    "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor 
    incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud 
    exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute 
    irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla 
    pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia 
    deserunt mollit anim id est laborum."
  `;

  const featuresData1: FeatureItem[] = [
    { title: "", image: "", link: "#laes" },
    { title: "", image: "", link: "#laer" },
    { title: "", image: "", link: "#spil" },
  ];

  const navRef = useRef<HTMLElement>(null);

  const smoothScroll = (targetId: string) => {
    const targetElement = document.getElementById(targetId);
    if (targetElement) {
      window.scrollTo({
        top: targetElement.offsetTop,
        behavior: "smooth",
      });
    }
  };

  const handleAnchorClick = (targetId: string) => {
    smoothScroll(targetId);
  };

  useEffect(() => {
    if (navRef.current) {
      const anchorLinks =
        navRef.current.querySelectorAll<HTMLAnchorElement>('a[href^="#"]');
      anchorLinks.forEach((link) => {
        link.addEventListener("click", (e) => {
          e.preventDefault();
          const targetId = (e.currentTarget as HTMLAnchorElement)
            ?.getAttribute("href")
            ?.substring(1);
          if (targetId) {
            handleAnchorClick(targetId);
          }
        });
      });
    }
  }, []);

  return (
    <div className={styles.landingPage}>
      <nav ref={navRef}>
        <ul>
          <li>
            <a
              href="#hero"
              onClick={(e) => {
                e.preventDefault();
                handleAnchorClick("hero");
              }}
            >
              Start
            </a>
          </li>
          <li>
            <a
              href="#first"
              onClick={(e) => {
                e.preventDefault();
                handleAnchorClick("first");
              }}
            >
              Introduktion
            </a>
          </li>
          <li>
            <a
              href="#mission"
              onClick={(e) => {
                e.preventDefault();
                handleAnchorClick("mission");
              }}
            >
              Mission
            </a>
          </li>
          <li>
            <a
              href="#features1"
              onClick={(e) => {
                e.preventDefault();
                handleAnchorClick("features1");
              }}
            >
              Funktioner
            </a>
          </li>
          <li>
            <a
              href="#team"
              onClick={(e) => {
                e.preventDefault();
                handleAnchorClick("team");
              }}
            >
              Team
            </a>
          </li>
          <li>
            <a
              href="#journey"
              onClick={(e) => {
                e.preventDefault();
                handleAnchorClick("journey");
              }}
            >
              Rejse
            </a>
          </li>
          <li>
            <a
              href="#contact"
              onClick={(e) => {
                e.preventDefault();
                handleAnchorClick("contact");
              }}
            >
              Kontakt
            </a>
          </li>
        </ul>
      </nav>
      //TODO: Indsæt navbar HER
      <HeroSection id="hero" />
      <FirstSection id="first" />
      <MissionSection id="mission" missionText={missionTekst} />
      <FeaturesSection id="features1" features={featuresData1} />
      <TeamSection id="team" />
      <JourneySection id="journey" />
      <Footer />
    </div>
  );
};

export default LandingPage;
