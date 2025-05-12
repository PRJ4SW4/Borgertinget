// src/components/Common/Button.tsx
import React, { ReactNode } from "react";
import styles from "./Button.module.css"; // Vi opretter denne CSS-modul fil

interface ButtonProps {
  children: ReactNode; // Tekst eller ikoner i knappen
  onClick?: () => void; // Valgfri funktion der kaldes ved klik
  className?: string; // Valgfri ekstra CSS-klasser
}

const Button: React.FC<ButtonProps> = ({ children, onClick, className }) => {
  return (
    <button className={`${styles.button} ${className || ""}`} onClick={onClick}>
      {children}
    </button>
  );
};

export default Button;
