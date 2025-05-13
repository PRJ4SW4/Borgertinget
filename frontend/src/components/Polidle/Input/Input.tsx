// src/components/Polidle/Input/Input.tsx
import React from "react";
import "./Input.module.css";

// Brug en type alias i stedet for et tomt interface
type GenericInputProps = React.InputHTMLAttributes<HTMLInputElement>;
// Hvis du *senere* vil tilføje custom props, kan du skifte tilbage til interface:
// interface GenericInputProps extends React.InputHTMLAttributes<HTMLInputElement> {
//   customProp?: string;
// }

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
