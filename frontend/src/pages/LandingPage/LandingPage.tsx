// src/pages/LandingPage/LandingPage.tsx
import React from "react";
import HeroSection from "../../components/LandingPage/Hero/HeroSection";
import FirstSection from "../../components/LandingPage/First/FirstSection";
import MissionSection from "../../components/LandingPage/Mission/MissionSection";
import FeaturesSection from "../../components/LandingPage/Features/FeaturesSection";
import TeamSection from "../../components/LandingPage/Team/TeamSection";
import JourneySection from "../../components/LandingPage/Journey/JourneySection";

import featureImage1 from "../../assets/features/feature_1.png";
import featureImage2 from "../../assets/features/feature_2.png";
import featureImage3 from "../../assets/features/feature_3.png";
import { useLandingPageLogic, FeatureItem } from "../../hooks/useLandingPage";

const featuresData1: FeatureItem[] = [
  { title: "", image: featureImage1, link: "#laes" },
  { title: "", image: featureImage2, link: "#laer" },
  { title: "", image: featureImage3, link: "#spil" },
];

const LandingPage: React.FC = () => {
  const { missionText } = useLandingPageLogic();

  return (
    <div>
      <HeroSection id="hero" />
      <FirstSection id="first" />
      <MissionSection id="mission" missionText={missionText} />
      <FeaturesSection id="features1" features={featuresData1} />
      <TeamSection id="team" />
      <JourneySection id="journey" />
    </div>
  );
};

export default LandingPage;
