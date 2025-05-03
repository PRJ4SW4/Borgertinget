import { useState, useEffect } from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/Login";
import Home from "./pages/Home";
import LearningLayout from './layouts/LearningEnvironment/LearningLayout';
import PageContent from './components/LearningEnvironment/PageContent';
import FlashcardLayout from './layouts/Flashcards/FlashcardLayout';
import CalendarView from './components/Calendar/CalendarView'
import LoginSuccessPage from './pages/LoginSuccessPage'; 
import PartyPage from "./pages/PartyPage";
import PoliticianPage from "./pages/PoliticianPage";
import PartiesPage from "./pages/PartiesPage";

function App() {
  const [token, setToken] = useState<string | null>(localStorage.getItem("jwt"));

  useEffect(() => {
    const handleStorageChange = () => {
      setToken(localStorage.getItem("jwt"));
    };

    window.addEventListener("storage", handleStorageChange);
    return () => window.removeEventListener("storage", handleStorageChange);
  }, []);

  return (
    <Routes>
      <Route path="/login" element={<Login setToken={setToken} />} />
      <Route path="/home" element={token ? <Home setToken={setToken} /> : <Navigate to="/login" />} />
      <Route path="/kalender" element={<CalendarView />} />

      <Route
          path="/learning"
          // Apply the SAME protection logic as /home
          element={token ? <LearningLayout /> : <Navigate to="/login" />}
        >
          <Route index element={<p>Velkommen til læringsområdet!</p>} />
          <Route path=":pageId" element={<PageContent />} />
      </Route>
      <Route
            path="/parties"
            element={<PartiesPage />}
          />
      <Route
            path="/party/:partyName"
            element={<PartyPage />}
          />
      <Route
            path="/politician/:id"
            element={<PoliticianPage />}
          />

      <Route
          path="/flashcards/*" // Match base path and potential nested paths
          element={token ? <FlashcardLayout /> : <Navigate to="/login" />}
      />

      <Route path="/login-success" element={<LoginSuccessPage setToken={setToken} />}/>
      <Route path="*" element={<Navigate to={token ? "/home" : "/login"} />} />
    </Routes>
  );
}

export default App;
