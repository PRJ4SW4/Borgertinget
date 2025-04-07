// src/components/LandingPage/Team/TeamSection.tsx
import React from "react";
import styles from "./TeamSection.module.css";

// Importer billederne (juster stierne så de passer til din projektstruktur)
import jakobLund from "../../../assets/team/jakob_lund.png";
import kevinNguyen from "../../../assets/team/kevin_nguyen.png";
import lasseFink from "../../../assets/team/lasse_fink.png";
import magnusHvidsten from "../../../assets/team/magnus_hvidsten.png";
import lucasStillborg from "../../../assets/team/lucas_stillborg.png";
import simonNowack from "../../../assets/team/simon_nowack.png";
import reneSchumacher from "../../../assets/team/rene_schumacher.png";
import oliviaHee from "../../../assets/team/olivia_hee.png"; // Brug .png hvis det er formatet

interface TeamMember {
  name: string;
  image: string;
}

interface TeamSectionProps {
  id?: string;
}

const teamMembers: TeamMember[] = [
  { name: "Jakob Lund", image: jakobLund },
  { name: "Kevin Nguyen", image: kevinNguyen },
  { name: "Lasse Fink", image: lasseFink },
  { name: "Magnus Hvidsten", image: magnusHvidsten },
  { name: 'Lucas "BatMan" Stillborg', image: lucasStillborg },
  { name: 'Simon "LatMan" Nowack', image: simonNowack },
  { name: 'Rene "DIF" Schumacher', image: reneSchumacher },
  { name: "Olivia Wass Hee", image: oliviaHee },
];

const TeamSection: React.FC<TeamSectionProps> = ({ id }) => {
  const teamDescription = `Vores team består af en række talentfulde og
passioneret software studerende som har påtaget
sig opgaven at bidrage til og uddanne den danske
ungdom indenfor politik`;

  return (
    <section id={id} className={styles.teamSection}>
      <div className={styles.container}>
        <h1>Mød vores team</h1>
        <p className={styles.description}>{teamDescription}</p>
        <div className={styles.membersGrid}>
          {teamMembers.map((member) => (
            <div key={member.name} className={styles.memberCard}>
              <img
                src={member.image}
                alt={member.name}
                className={styles.memberImage}
              />
              <h3 className={styles.memberName}>{member.name}</h3>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
};

export default TeamSection;
