import React, { useEffect, useRef, useCallback } from "react";
import HeroSection from "../../components/LandingPage/Hero/HeroSection";
import FirstSection from "../../components/LandingPage/First/FirstSection";
import MissionSection from "../../components/LandingPage/Mission/MissionSection";
import FeaturesSection from "../../components/LandingPage/Features/FeaturesSection";
import TeamSection from "../../components/LandingPage/Team/TeamSection";
import JourneySection from "../../components/LandingPage/Journey/JourneySection";

import featureImage1 from "../../assets/features/feature_1.png";
import featureImage2 from "../../assets/features/feature_2.png";
import featureImage3 from "../../assets/features/feature_3.png";

interface FeatureItem {
  title: string;
  image: string;
  link: string;
}

const LandingPage: React.FC = () => {
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
    { title: "", image: featureImage1, link: "#laes" },
    { title: "", image: featureImage2, link: "#laer" },
    { title: "", image: featureImage3, link: "#spil" },
  ];

  const navRef = useRef<HTMLElement>(null);

  const smoothScroll = useCallback((targetId: string) => {
    const targetElement = document.getElementById(targetId);
    if (targetElement) {
      window.scrollTo({
        top: targetElement.offsetTop,
        behavior: "smooth",
      });
    }
  }, []);

  const handleAnchorClick = useCallback((targetId: string) => {
    smoothScroll(targetId);
  }, [smoothScroll]);

  useEffect(() => {
    const currentNavRef = navRef.current;
    if (currentNavRef) {
      const anchorLinks =
        currentNavRef.querySelectorAll<HTMLAnchorElement>('a[href^="#"]');
      
      const listeners: { element: HTMLAnchorElement; handler: (event: MouseEvent) => void }[] = [];

      anchorLinks.forEach((link) => {
        const eventHandler = (e: MouseEvent) => {
          e.preventDefault();
          const targetId = (e.currentTarget as HTMLAnchorElement)
            ?.getAttribute("href")
            ?.substring(1);
          if (targetId) {
            handleAnchorClick(targetId);
          }
        };
        link.addEventListener("click", eventHandler);
        listeners.push({ element: link, handler: eventHandler });
      });

      return () => {
        listeners.forEach(({ element, handler }) => {
          element.removeEventListener("click", handler);
        });
      };
    }
  }, [handleAnchorClick]);

  return (
    <div>
      <HeroSection id="hero" />
      <FirstSection id="first" />
      <MissionSection id="mission" missionText={missionTekst} />
      <FeaturesSection id="features1" features={featuresData1} />
      <TeamSection id="team" />
      <JourneySection id="journey" />
    </div>
  );
};

export default LandingPage;
