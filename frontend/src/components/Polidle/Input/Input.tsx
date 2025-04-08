import React, { useState } from "react";
import "./Input.css";

interface InputProps {
  onGuess: (guess: {
    politikker: string;
    køn: string;
    parti: string;
    alder: number;
    region: string;
    uddannelse: string;
  }) => void;
}

const Input: React.FC<InputProps> = ({ onGuess }) => {
  const [inputText, setInputText] = useState("");

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setInputText(event.target.value);
  };

  const handleSubmit = () => {
    onGuess({
      politikker: inputText,
      køn: "",
      parti: "",
      alder: 0,
      region: "",
      uddannelse: "",
    }); // Tilpas gætteobjektet
    setInputText(""); // Ryd inputfeltet
  };

  return (
    <div>
      <input
        type="text"
        className="input-field"
        placeholder="Gæt en politiker"
        value={inputText}
        onChange={handleInputChange}
      />
      <button onClick={handleSubmit}>Gæt</button>
    </div>
  );
};

export default Input;
