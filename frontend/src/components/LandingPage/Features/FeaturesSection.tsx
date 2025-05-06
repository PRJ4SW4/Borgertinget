// src/components/LandingPage/Features/FeaturesSection.tsx
import React from "react";
import styles from "./FeaturesSection.module.css";

interface FeatureItem {
  title: string;
  image: string;
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
