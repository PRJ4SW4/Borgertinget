// src/components/LandingPage/Features/FeaturesSection.tsx
import React from "react";
import styles from "./FeaturesSection.module.css";
import Button from "../../common/Button"; // Antager din genbrugelige knapkomponent
// Importer dine tre billeder (juster stierne)
import featureImage1 from "../../../assets/features/feature_1.png";
import featureImage2 from "../../../assets/features/feature_2.jpg";
import featureImage3 from "../../../assets/features/feature_3.png";

interface FeatureItem {
  title: string;
  image: string;
  link: string; // Eller en onClick handler, afhængigt af din routing
}

interface FeaturesSectionProps {
  id?: string;
  features: FeatureItem[];
}

const FeaturesSection: React.FC<FeaturesSectionProps> = ({ id, features }) => {
  return (
    <section id={id} className={styles.featuresSection}>
      <div className={styles.container}>
        <h1>En moderne tilgang til politik</h1>
        <div className={styles.featuresGrid}>
          {features.map((feature, index) => (
            <div key={index} className={styles.featureItem}>
              <Button onClick={() => (window.location.href = feature.link)}>
                {feature.title}
              </Button>
              <div className={styles.imageContainer}>
                <img
                  src={feature.image}
                  alt={feature.title}
                  className={styles.featureImage}
                />
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
};

export default FeaturesSection;
