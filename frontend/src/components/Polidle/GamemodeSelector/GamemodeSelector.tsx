// src/components/Polidle/GamemodeSelector/GamemodeSelector.tsx
import React from "react";
import { NavLink } from "react-router-dom"; // <<< Brug NavLink
import styles from "./GamemodeSelector.module.css";
import { GamemodeTypes } from "../../../types/PolidleTypes"; // <<< Importer GameMode enum

// Definer en type for et gamemode objekt
interface GamemodeInfo {
  id: GamemodeTypes; // Brug enum for ID
  name: string; // Navn til evt. aria-label eller tooltip
  path: string;
  symbol: string;
}

// Definer gamemodes data
// Path'ene matcher dine routes i App.tsx
const GAMEMODES_CONFIG: GamemodeInfo[] = [
  {
    id: GamemodeTypes.Klassisk,
    name: "Klassisk Mode",
    path: "/ClassicMode",
    symbol: "❔",
  },
  {
    id: GamemodeTypes.Citat,
    name: "Citat Mode",
    path: "/CitatMode",
    symbol: "❝❞",
  },
  {
    id: GamemodeTypes.Foto,
    name: "Foto Mode",
    path: "/FotoBlurMode",
    symbol: "📸️",
  },
];

// Komponentenavnet bør være PascalCase
const GamemodeSelector: React.FC = () => {
  // useLocation er ikke længere nødvendig, hvis NavLink håndterer active class

  return (
    <nav className={styles.gameSelector} aria-label="Vælg spiltype">
      {" "}
      {/* Tilføjet aria-label */}
      {GAMEMODES_CONFIG.map((mode) => (
        <NavLink
          key={mode.id}
          to={mode.path}
          // NavLink kan tage en funktion til className for at sætte active class
          className={({ isActive }) =>
            `${styles.gameSelectorButton} ${isActive ? styles.active : ""}`
          }
          title={mode.name} // Tooltip for tilgængelighed og UX
          aria-label={`Skift til ${mode.name}`} // Bedre tilgængelighed
        >
          <span aria-hidden="true">{mode.symbol}</span>{" "}
          {/* Skjul symbol for skærmlæsere hvis navn er nok */}
        </NavLink>
      ))}
    </nav>
  );
};

export default GamemodeSelector;
