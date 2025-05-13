// src/pages/Polidle/Polidle.tsx
import React from "react"; // Fjernet useState, da selectedGamemode fjernes
import { Link } from "react-router-dom";

import { GamemodeTypes } from "../../types/PolidleTypes"; // Importer GameMode enum

import styles from "./PolidlePage.module.css";

// Definer en type for et gamemode objekt i denne hub-side
interface HubGamemodeInfo {
  id: GamemodeTypes; // Eller string hvis du foretrækker det til key, men enum er mere type-sikkert ift. GameMode
  name: string;
  path: string;
  symbol: string;
  description: string;
}

// Definer gamemodes data
// Sørg for at path matcher dine routes i App.tsx
const GAMEMODES_HUB_CONFIG: HubGamemodeInfo[] = [
  {
    id: GamemodeTypes.Klassisk, // Bruger enum
    name: "Klassisk",
    path: "/ClassicMode", // Sørg for denne route eksisterer i App.tsx
    symbol: "❓",
    description: "Få ledetråde om politikerens parti, alder, køn m.m.",
  },
  {
    id: GamemodeTypes.Citat, // Bruger enum
    name: "Citat",
    path: "/CitatMode", // Sørg for denne route eksisterer i App.tsx
    symbol: "❝❞",
    description: "Gæt politikeren bag et kendt (eller ukendt) citat.",
  },
  {
    id: GamemodeTypes.Foto, // Bruger enum
    name: "Foto Sløret", // Opdateret navn
    path: "/FotoBlurMode", // Sørg for denne route eksisterer i App.tsx
    symbol: "📸️",
    description: "Gæt hvem der gemmer sig bag det slørede billede.",
  },
];

const PolidlePage: React.FC = () => {
  return (
    <div className={styles.container}>
      {" "}
      {/* Bruger styles.container */}
      <h1 className={styles.heading}>Polidle🇩🇰</h1>{" "}
      {/* Bruger styles.heading */}
      <p className={styles.subheading}>
        Vælg en spiltype for Dagens Polidle
      </p>{" "}
      {/* Bruger styles.subheading */}
      <div className={styles.gamemodeList}>
        {" "}
        {/* Bruger styles.gamemodeList */}
        {GAMEMODES_HUB_CONFIG.map((mode) => (
          <Link
            key={mode.id}
            to={mode.path}
            className={styles.gamemodeButton} // Bruger styles.gamemodeButton
          >
            <span className={styles.gamemodeSymbol} aria-hidden="true">
              {mode.symbol}
            </span>{" "}
            {/* Bruger styles.gamemodeSymbol */}
            <div className={styles.gamemodeText}>
              {" "}
              {/* Bruger styles.gamemodeText */}
              <span className={styles.gamemodeName}>{mode.name}</span>{" "}
              {/* Bruger styles.gamemodeName */}
              <span className={styles.gamemodeDescription}>
                {" "}
                {/* Bruger styles.gamemodeDescription */}
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
