import React, { useState } from "react";
import { Link } from "react-router-dom";
import styles from "./Polidle.module.css";

const Polidle: React.FC = () => {
  const [selectedGamemode, setSelectedGamemode] = useState<string | null>(null);

  const gamemodes = [
    {
      id: "classic",
      name: "Klassisk",
      path: "/ClassicMode",
      symbol: "❓",
      description: "Få ledetråde ved hvert forsøg",
    },
    {
      id: "citat",
      name: "Citat",
      path: "/CitatMode",
      symbol: "❝❞",
      description: "Gæt med citater i spillet",
    },
    {
      id: "foto-blur",
      name: "FotoBlue",
      path: "/FotoBlurMode",
      symbol: "📸️",
      description: "Gæt ud fra en sektion af billedet",
    },
  ];

  return (
    <div className={styles.container}>
      <h1 className={styles.heading}>Polidle🇩🇰</h1>
      <p className={styles.subheading}>Vælg en gamemode for Dagens Polidle</p>
      {selectedGamemode ? (
        <div>
          <h2>
            Du spiller nu{" "}
            {gamemodes.find((mode) => mode.id === selectedGamemode)?.name}
          </h2>
          <Link to="/" className={styles.backButton}>
            Tilbage til gamemode-valg
          </Link>
        </div>
      ) : (
        <div className={styles.gamemodeList}>
          {gamemodes.map((mode) => (
            <Link
              key={mode.id}
              to={mode.path}
              className={styles.gamemodeButton}
              onClick={() => setSelectedGamemode(mode.id)}
            >
              <span className={styles.gamemodeSymbol}>{mode.symbol}</span>
              <div className={styles.gamemodeText}>
                <span className={styles.gamemodeName}>{mode.name}</span>
                <span className={styles.gamemodeDescription}>
                  {mode.description}
                </span>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
};

export default Polidle;
