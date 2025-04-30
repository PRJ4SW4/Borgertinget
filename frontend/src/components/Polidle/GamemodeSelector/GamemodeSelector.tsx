import React from "react";
import { Link, useLocation } from "react-router-dom";
import styles from "./GamemodeSelector.module.css"; // Opdateret import

const GameSelector: React.FC = () => {
  const location = useLocation();

  const gamemodes = [
    { id: "classic", path: "/ClassicMode", symbol: "❔" },
    { id: "citat", path: "/CitatMode", symbol: "❝❞" },
    { id: "foto", path: "/FotoBlurMode", symbol: "📸️" },
  ];

  return (
    <div className={styles.gameSelector}>
      {gamemodes.map((mode) => (
        <Link
          key={mode.id}
          to={mode.path}
          className={`${styles.gameSelectorButton} ${
            location.pathname === mode.path ? styles.active : ""
          }`}
        >
          {mode.symbol}
        </Link>
      ))}
    </div>
  );
};

export default GameSelector;
