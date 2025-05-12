// src/components/Polidle/Input/Input.tsx
import React from "react";
import "./Input.module.css"; // Din eksisterende styling for .input-field

// Udvid standard HTML input attributter for fuld fleksibilitet og type-sikkerhed
interface GenericInputProps
  extends React.InputHTMLAttributes<HTMLInputElement> {
  // Vi kan stadig definere custom props her, hvis de IKKE er standard HTML attributter.
  // F.eks. hvis du ville have en 'variant' prop til forskellige udseender:
  // variant?: 'primary' | 'search' | 'error';
  // Men for nu er standardattributterne dækkende.
}

const Input: React.FC<GenericInputProps> = ({
  className = "", // Tillad ekstern className og hav en default
  type = "text", // Default type til "text"
  ...rest // Samler alle andre props (value, onChange, placeholder, disabled, osv.)
}) => {
  return (
    <input
      type={type}
      // Kombinerer den faste .input-field klasse med eventuelle eksterne klasser
      className={`input-field ${className}`.trim()}
      {...rest} // Spreder alle props videre til det native input-element
    />
  );
};

export default Input;
