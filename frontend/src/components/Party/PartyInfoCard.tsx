import React from 'react';
import './PartyInfoCard.css'; // We'll create this CSS file next

// Define the properties the component expects
interface PartyInfoCardProps {
  partyName: string;
  slogan?: string; // Slogan is optional as it's not in the current backend model
  logoUrl?: string; // URL for the party logo, resolved by the parent
  chairmanName?: string | null; // Partiformand
  viceChairmanName?: string | null; // Næstformand (Party Deputy)
  secretaryName?: string | null; // Partisekretær
  politicalSpokespersonName?: string | null; // Politisk ordfører
  groupLeaderName?: string | null; // Gruppeformand
  deputyChairmanName2?: string | null; // Second "Næstformand" shown in the design
  defaultLogo: string; // Fallback logo path (import this in the parent)
}

const PartyInfoCard: React.FC<PartyInfoCardProps> = ({
  partyName,
  slogan,
  logoUrl,
  chairmanName,
  viceChairmanName,
  secretaryName,
  politicalSpokespersonName,
  groupLeaderName,
  deputyChairmanName2,
  defaultLogo, // Use the passed-in default logo
}) => {

  // Helper function to render role information cleanly
  const renderRole = (label: string, name: string | null | undefined) => {
    return (
      <div className="party-role">
        <span className="party-role-label">{label}:</span>
        {/* Display the name or a placeholder if null/undefined */}
        <span className="party-role-name">{name || '[Ikke angivet]'}</span>
      </div>
    );
  };

  // Handle image loading errors
  const handleImageError = (e: React.SyntheticEvent<HTMLImageElement, Event>) => {
    const target = e.target as HTMLImageElement;
    if (target.src !== defaultLogo) {
      console.warn(`Failed to load party logo: ${logoUrl}. Using default.`);
      target.src = defaultLogo; // Attempt to load default logo
    } else {
      console.error(`Failed to load default party logo: ${defaultLogo}`);
      // Optional: Hide the image element completely if default also fails
      // target.style.display = 'none';
    }
  };

  return (
    <div className="party-info-card">
      {/* Party Name */}
      <h1 className="party-info-name">{partyName}</h1>

      {/* Slogan (Optional) */}
      {slogan && <p className="party-info-slogan">{slogan}</p>}

      {/* Logo */}
      <div className="party-info-logo-container">
        <img
          src={logoUrl || defaultLogo} // Use provided logo or fallback to default
          alt={`${partyName} Logo`}
          className="party-info-logo"
          onError={handleImageError} // Use the error handler
        />
      </div>

      {/* Party Structure Section */}
      <h2 className="party-info-section-title">Parti opbygning</h2>
      <div className="party-info-roles">
        {renderRole('Partiformand', chairmanName)}
        {renderRole('Næstformand', viceChairmanName)}
        {renderRole('Partisekretær', secretaryName)}
        {renderRole('Politisk ordfører', politicalSpokespersonName)}
        {renderRole('Gruppeformand', groupLeaderName)}
        {/* Render the second "Næstformand" as shown in the design */}
        {renderRole('Næstformand', deputyChairmanName2)}
      </div>

      {/* Divider Line */}
      <hr className="party-info-divider" />
    </div>
  );
};

export default PartyInfoCard;