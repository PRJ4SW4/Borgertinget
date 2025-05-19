// src/pages/Polidle/PolidlePage.tsx
import React from "react";
import { Link } from "react-router-dom";

import styles from "./PolidlePage.module.css";
import { GAMEMODES_HUB_CONFIG } from "./PolidlePage.logic";

const PolidlePage: React.FC = () => {
  return (
    <div className={styles.container}>
      <h1 className={styles.heading}>Polidle🇩🇰</h1>
      <p className={styles.subheading}>Vælg en spiltype for Dagens Polidle</p>
      <div className={styles.gamemodeList}>
        {GAMEMODES_HUB_CONFIG.map((mode) => (
          <Link key={mode.id} to={mode.path} className={styles.gamemodeButton}>
            <span className={styles.gamemodeSymbol} aria-hidden="true">
              {mode.symbol}
            </span>
            <div className={styles.gamemodeText}>
              <span className={styles.gamemodeName}>{mode.name}</span>
              <span className={styles.gamemodeDescription}>
                {mode.description}
              </span>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
};

export default PolidlePage;
