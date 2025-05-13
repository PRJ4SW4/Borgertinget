// src/App.tsx
import { useState, useEffect, JSX } from "react";
import { Routes, Route, Navigate } from "react-router-dom";

// Layout Components
import LearningLayout from "./layouts/LearningEnvironment/LearningLayout";
import FlashcardLayout from "./layouts/Flashcards/FlashcardLayout";
import MainLayout from "./layouts/MainLayout";
import NavbarLandingPageLayout from "./layouts/LandingPage/NavbarLandingPageLayout";

// Page Components
import Login from "./pages/Login";
import HomePage from "./pages/HomePage/HomePage";
import PageContent from "./components/LearningEnvironment/PageContent";
import CalendarView from "./components/Calendar/CalendarView";
import PartyPage from "./pages/PartyPage";
import PoliticianPage from "./pages/PoliticianPage";
import PartiesPage from "./pages/PartiesPage";
import LandingPage from "./pages/LandingPage/LandingPage";
import FeedPage from "./pages/FeedPage";

// Polidle Pages - Sørg for korrekte importstier
import PolidlePage from "./pages/PolidlePage/PolidlePage"; // Din hub-side
import ClassicMode from "./pages/PolidlePage/ClassicMode/ClassicMode";
import QuoteMode from "./pages/PolidlePage/QuoteMode/QuoteMode";
import FotoBlurMode from "./pages/PolidlePage/FotoBlurMode/FotoBlurMode";

import EmailVerification from "./utils/useEmailVerification";

function App() {
  const [token, setToken] = useState<string | null>(
    localStorage.getItem("jwt")
  );
  const handleSetToken = (newToken: string | null) => {
    setToken(newToken);
    if (newToken) {
      localStorage.setItem("jwt", newToken);
    } else {
      localStorage.removeItem("jwt");
    }
  };

  useEffect(() => {
    const handleStorageChange = () => {
      setToken(localStorage.getItem("jwt"));
    };
    window.addEventListener("storage", handleStorageChange);
    return () => window.removeEventListener("storage", handleStorageChange);
  }, []);

  const ProtectedRoute: React.FC<{ children: JSX.Element }> = ({
    children,
  }) => {
    return token ? children : <Navigate to="/login" />;
  };

  return (
    <Routes>
      {/* --- Public Navbar Route --- */}
      <Route element={<NavbarLandingPageLayout />}>
        {" "}
        {/* Fjernet </NavbarLandingPageLayout> her */}
        <Route path="/LandingPage" element={<LandingPage />} />
      </Route>{" "}
      {/* <<< Lukket </Route> her */}
      {/* --- Public Routes (No MainLayout, No login required) --- */}
      <Route path="/login" element={<Login setToken={handleSetToken} />} />
      <Route path="/login-success" element={<></>} />{" "}
      {/* Typisk redirect efter OAuth */}
      <Route
        path="/verify"
        element={<EmailVerification onVerified={() => {}} onError={() => {}} />}
      />
      {/* --- Protected Routes using MainLayout --- */}
      <Route
        element={
          <ProtectedRoute>
            <MainLayout />
          </ProtectedRoute>
        }
      >
        {/* Standard HomePage efter login */}
        <Route path="/homepage" element={<HomePage />} />
        {/* Rod-stien for logged-in brugere, navigerer til /homepage */}
        {/* Overvej om '/' skal være HomePage eller LandingPage afhængig af login status.
            Den nuværende catch-all håndterer dette, men en eksplicit '/' kan være klarere. */}
        {/* <Route path="/" element={<Navigate to="/homepage" replace />} /> */}
        {/* Andre beskyttede sider */}
        <Route path="/kalender" element={<CalendarView />} />
        <Route path="/feed" element={<FeedPage />} />{" "}
        {/* Sikret at FeedPage er her */}
        <Route path="/parties" element={<PartiesPage />} />
        <Route path="/party/:partyName" element={<PartyPage />} />
        <Route path="/politician/:id" element={<PoliticianPage />} />
        {/* --- Learning Environment Routes --- */}
        <Route path="/learning" element={<LearningLayout />}>
          <Route
            index
            element={
              <p>Velkommen til læringsområdet! Vælg et emne i menuen.</p>
            }
          />
          <Route path=":pageId" element={<PageContent />} />
        </Route>
        {/* --- Flashcards Routes --- */}
        <Route path="/flashcards/*" element={<FlashcardLayout />} />
        {/* **************************************************** */}
        {/* *** START: Polidle Routes (Beskyttet & i MainLayout) *** */}
        {/* **************************************************** */}
        <Route path="/polidle" element={<PolidlePage />} />
        <Route path="/ClassicMode" element={<ClassicMode />} />
        <Route path="/CitatMode" element={<QuoteMode />} />{" "}
        {/* Props er fjernet */}
        <Route path="/FotoBlurMode" element={<FotoBlurMode />} />{" "}
        {/* Props er fjernet */}
        {/* ************************************************** */}
        {/* *** SLUT: Polidle Routes                         *** */}
        {/* ************************************************** */}
      </Route>{" "}
      {/* End of Protected MainLayout routes */}
      {/* --- Catch-all og Root Redirects --- */}
      {/* Håndterer rod-stien: hvis logget ind -> /homepage, ellers -> /landingpage */}
      <Route
        path="/"
        element={<Navigate to={token ? "/homepage" : "/landingpage"} replace />}
      />
      {/* Catch-all for ukendte stier: hvis logget ind -> /homepage, ellers -> /landingpage */}
      <Route
        path="*"
        element={<Navigate to={token ? "/homepage" : "/landingpage"} replace />}
      />
    </Routes>
  );
}

export default App;
