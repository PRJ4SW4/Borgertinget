// src/components/LandingPage/Team/TeamSection.tsx (eller en separat typefil)
import React from "react";

interface TeamMember {
  name: string;
  role: string;
  image: string;
  // ... andre felter
}

interface TeamSectionProps {
  members: TeamMember[];
}

const TeamSection: React.FC<TeamSectionProps> = ({ members }) => {
  return (
    <section id="team">
      <h2>Vores Team</h2>
      {members.map((member) => (
        <div key={member.name}>
          <img src={member.image} alt={member.name} />
          <h3>{member.name}</h3>
          <p>{member.role}</p>
        </div>
      ))}
    </section>
  );
};

export default TeamSection;
