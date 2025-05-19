// src/pages/Polidle/PolidlePage.logic.ts
import { GamemodeTypes } from "../../types/PolidleTypes";

export interface HubGamemodeInfo {
  id: GamemodeTypes;
  name: string;
  path: string;
  symbol: string;
  description: string;
}

export const GAMEMODES_HUB_CONFIG: HubGamemodeInfo[] = [
  {
    id: GamemodeTypes.Klassisk,
    name: "Klassisk",
    path: "/ClassicMode",
    symbol: "❓",
    description: "Få ledetråde om politikerens parti, alder, køn m.m.",
  },
  {
    id: GamemodeTypes.Citat,
    name: "Citat",
    path: "/CitatMode",
    symbol: "❝❞",
    description: "Gæt politikeren bag et kendt (eller ukendt) citat.",
  },
  {
    id: GamemodeTypes.Foto,
    name: "Foto",
    path: "/FotoBlurMode",
    symbol: "📸️",
    description: "Gæt hvem der gemmer sig bag det slørede billede.",
  },
];
