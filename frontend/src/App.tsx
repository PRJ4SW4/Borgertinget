// src/App.tsx
import { useState, useEffect, JSX } from "react";
// Imports components from react-router-dom for routing.
import { Routes, Route, Navigate } from "react-router-dom";

// Layout Components
import LearningLayout from "./layouts/LearningEnvironment/LearningLayout";
import FlashcardLayout from "./layouts/Flashcards/FlashcardLayout";
import MainLayout from "./layouts/MainLayout"; // Navbar and Footer are rendered via MainLayout.
import NavbarLandingPageLayout from "./layouts/LandingPage/NavbarLandingPageLayout"; // Different layout for the landing page.

// Page Components
import Login from "./pages/Login";
import HomePage from "./pages/HomePage/HomePage";
import PageContent from './components/LearningEnvironment/PageContent'; // Renders content within LearningLayout.
import CalendarView from './components/Calendar/CalendarView';
import PartyPage from "./pages/PartyPage"; // Displays details for a specific party.
import PoliticianPage from "./pages/PoliticianPage"; // Displays details for a specific politician.
import PartiesPage from "./pages/PartiesPage"; // Displays a list of parties.
import LandingPage from "./pages/LandingPage/LandingPage";
import FeedPage from "./pages/FeedPage";

// Polidle Pages
import PolidlePage from "./pages/PolidlePage/PolidlePage";
import ClassicMode from "./pages/PolidlePage/ClassicMode/ClassicMode";
import QuoteMode from "./pages/PolidlePage/QuoteMode/QuoteMode";
import FotoBlurMode from "./pages/PolidlePage/FotoBlurMode/FotoBlurMode";

import EmailVerification from "./utils/useEmailVerification"; // Handles email verification logic.
import LoginSuccessPage from "./pages/LoginSuccessPage"; // Handles post-login redirection.
// Navbar and Footer are rendered via MainLayout.
// The main application component.
function App() {
  
  // State hook for the JWT authentication token.
  // Initializes state from localStorage to persist login status.
  const [token, setToken] = useState<string | null>(localStorage.getItem("jwt"));
  const handleSetToken = (newToken: string | null) => {
    setToken(newToken);
    if (newToken) {
      localStorage.setItem("jwt", newToken);
    } else {
      localStorage.removeItem("jwt");
    }
  };
// Effect hook to synchronize token state with localStorage changes across tabs/windows.
  useEffect(() => {
    const handleStorageChange = () => {
      // Updates the component's token state when localStorage changes.
      setToken(localStorage.getItem("jwt"));
    };
    // Adds the event listener on component mount.
    window.addEventListener("storage", handleStorageChange);
    // Removes the event listener on component unmount to prevent memory leaks.
    return () => window.removeEventListener("storage", handleStorageChange); 
  }, []);  // Empty dependency array ensures the effect runs only on mount and unmount.

  // --- Protected Route Component ---
  // Wraps routes that require user authentication.
  // Renders the child component if a token exists, otherwise redirects to /login.
  const ProtectedRoute: React.FC<{ children: JSX.Element }> = ({
    children,
  }) => {
    return token ? children : <Navigate to="/login" />;
  };

  // --- Routing Setup ---
  // Defines the application's routes using the Routes component.
  return (
    <Routes>
      {/* --- Public Navbar Route --- */}
      <Route element={<NavbarLandingPageLayout />}>
        {" "}
        <Route path="/LandingPage" element={<LandingPage />} />
      </Route>{" "}
      {/* <<< Lukket </Route> her */}
      {/* --- Public Routes (No MainLayout, No login required) --- */}
      <Route path="/login" element={<Login setToken={handleSetToken} />} />
      {/* Post-login success/callback route. */}
      <Route path="/login-success" element={<LoginSuccessPage setToken={handleSetToken} />} /> 
      <Route path="/verify" element={<EmailVerification onVerified={() => {}} onError={(message) => { console.error("Email verification error:", message); }}/>} />


      {/* --- Protected Routes using MainLayout --- */}
      {/* This Route group uses MainLayout and requires authentication via ProtectedRoute. */}
      {/* All nested routes inherit the layout and protection. */}
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
        {/* --- START: Polidle Routes (Beskyttet & i MainLayout) --- */}
        <Route path="/polidle" element={<PolidlePage />} />
        <Route path="/ClassicMode" element={<ClassicMode />} />
        <Route path="/CitatMode" element={<QuoteMode />} />{" "}
        <Route path="/FotoBlurMode" element={<FotoBlurMode />} />{" "}
        {/* --- SLUT: Polidle Routes --- */}
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
