import { useState, useEffect } from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/Login";
import Home from "./pages/Home";
import Verify from "./pages/Verify";
import LearningLayout from './layouts/LearningLayout';
import PageContent from './components/PageContent';
import FlashcardLayout from './layouts/FlashcardLayout'; // Import new layout

import Polidle from "./pages/Polidle/Polidle";
// Importet gamemodes
import ClassicMode from "./pages/Polidle/ClassicMode";
import CitatMode from "./pages/Polidle/CitatMode";
import FotoBlurMode from "./pages/Polidle/FotoBlurMode";

function App() {
  const [token, setToken] = useState<string | null>(
    localStorage.getItem("jwt")
  );

  useEffect(() => {
    const handleStorageChange = () => {
      setToken(localStorage.getItem("jwt"));
    };

    window.addEventListener("storage", handleStorageChange);
    return () => window.removeEventListener("storage", handleStorageChange);
  }, []);

  // Placeholder for politikerImage - husk at erstatte med faktiske billeder senere
  const placeholderImage = ""; // Eller en URL til et standardbillede

  return (
    <Routes>
      <Route path="/login" element={<Login setToken={setToken} />} />
      <Route
        path="/home"
        element={
          token ? <Home setToken={setToken} /> : <Navigate to="/login" />
        }
      />
      <Route path="/verify" element={<Verify />} />

      <Route
          path="/learning"
          // Apply the SAME protection logic as /home if needed
          // If learning is public, just use: element={<LearningLayout />}
          element={token ? <LearningLayout /> : <Navigate to="/login" />}
        >
          {/* Nested routes render inside LearningLayout's <Outlet /> */}
          <Route index element={<p>Velkommen til læringsområdet!</p>} />
          <Route path=":pageId" element={<PageContent />} />
      </Route>

      <Route
          path="/flashcards/*" // Match base path and potential nested paths
          element={token ? <FlashcardLayout /> : <Navigate to="/login" />}
      />

      <Route path="*" element={<Navigate to={token ? "/home" : "/login"} />} />
      <Route path="/Polidle" element={<Polidle />} />
      {/* gamemodes */}
      <Route path="/ClassicMode" element={<ClassicMode />} />
      <Route
        path="/CitatMode"
        element={
          <CitatMode
            citat="Sample Citat"
            correctPolitiker="Sample Politician"
            politikerImage={placeholderImage} // Tilføjet politikerImage prop
          />
        }
      />
      <Route
        path="/FotoBlurMode"
        element={
          <FotoBlurMode
            imageUrl="sample-image-url.jpg"
            correctPolitiker="Sample Politician"
            politikerImage={placeholderImage} // Tilføjet politikerImage prop
          />
        }
      />
    </Routes>
  );
}

export default App;
