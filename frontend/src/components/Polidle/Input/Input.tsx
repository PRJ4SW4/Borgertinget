// src/components/Polidle/Input/Input.tsx
import React from "react";
import "./Input.module.css";

// Brug en type alias i stedet for et tomt interface
type GenericInputProps = React.InputHTMLAttributes<HTMLInputElement>;

const Input: React.FC<GenericInputProps> = ({
  className = "",
  type = "text",
  ...rest
}) => {
  return (
    <input
      type={type}
      className={`input-field ${className}`.trim()}
      {...rest}
    />
  );
};

export default Input;
