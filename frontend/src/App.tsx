import { useState, useEffect, JSX } from "react";
// Imports components from react-router-dom for routing.
import { Routes, Route, Navigate } from "react-router-dom";

// Layout Components: Provide consistent page structure.
import LearningLayout from './layouts/LearningEnvironment/LearningLayout';
import FlashcardLayout from './layouts/Flashcards/FlashcardLayout';
import MainLayout from './layouts/MainLayout'; // Standard layout with Navbar/Footer.

// Page Components: Represent different views/pages in the application.
import Login from "./pages/Login";
// Represents the main view for authenticated users.
import Home from "./pages/Home";
// HomePage after user signs in.
import HomePage from "./pages/HomePage/HomePage";
import PageContent from './components/LearningEnvironment/PageContent'; // Renders content within LearningLayout.
import CalendarView from './components/Calendar/CalendarView';
import LoginSuccessPage from './pages/LoginSuccessPage';
import PartyPage from "./pages/PartyPage"; // Displays details for a specific party.
import PoliticianPage from "./pages/PoliticianPage"; // Displays details for a specific politician.
import PartiesPage from "./pages/PartiesPage"; // Displays a list of parties.
// Navbar and Footer are rendered via MainLayout.

// The main application component.
function App() {
  // State hook for the JWT authentication token.
  // Initializes state from localStorage to persist login status.
  const [token, setToken] = useState<string | null>(localStorage.getItem("jwt"));

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
  }, []); // Empty dependency array ensures the effect runs only on mount and unmount.

  // --- Protected Route Component ---
  // Wraps routes that require user authentication.
  // Renders the child component if a token exists, otherwise redirects to /login.
  const ProtectedRoute: React.FC<{ children: JSX.Element }> = ({ children }) => {
    return token ? children : <Navigate to="/login" />;
  };


  // --- Routing Setup ---
  // Defines the application's routes using the Routes component.
  return (
    <Routes>
      {/* --- Public Routes (No MainLayout, No login required) --- */}
      {/* Login page route. */}
      <Route path="/login" element={<Login setToken={setToken} />} />
      {/* Post-login success/callback route. */}
      <Route path="/login-success" element={<LoginSuccessPage setToken={setToken} />}/>


      {/* --- Protected Routes using MainLayout --- */}
      {/* This Route group uses MainLayout and requires authentication via ProtectedRoute. */}
      {/* All nested routes inherit the layout and protection. */}
      <Route element={<ProtectedRoute><MainLayout /></ProtectedRoute>}>
        {/* Root path ("/") route, shows HomePage for logged-in users. */}
        <Route path="/" element={<HomePage />} />

        {/* Home is an old route for a previous homepage, should be removed for production environment */}
        <Route
            path="/home"
            element={<Home setToken={setToken} />} // Pass setToken for logout functionality within Home.
        />

        {/* Calendar route (requires login). */}
        <Route
            path="/kalender"
            element={<CalendarView />}
        />

        {/* Other routes requiring login and using MainLayout. */}
        <Route path="/parties" element={<PartiesPage />} />
        <Route path="/party/:partyName" element={<PartyPage />} /> {/* ':partyName' is a dynamic URL parameter. */}
        <Route path="/politician/:id" element={<PoliticianPage />} /> {/* ':id' is a dynamic URL parameter. */}

        {/* --- Learning Environment Routes (Nested and Protected) --- */}
        {/* This route group uses LearningLayout and is protected by the parent ProtectedRoute. */}
        <Route
          path="/learning"
          element={<LearningLayout />} // Uses its own layout in addition to MainLayout, protection inherited.
        >
          {/* Default content shown at "/learning". */}
          <Route index element={<p>Velkommen til læringsområdet! Vælg et emne i menuen.</p>} />
          {/* Route for specific learning pages, e.g., "/learning/topic-1". */}
          <Route path=":pageId" element={<PageContent />} /> {/* ':pageId' is a dynamic URL parameter. */}
        </Route>

        {/* --- Flashcards Routes (Nested and Protected) --- */}
        {/* Uses FlashcardLayout, protection inherited. "/*" enables nested routing within FlashcardLayout. */}
        <Route
            path="/flashcards/*"
            element={<FlashcardLayout />}
        />

        {/* If others need to define other protected routes using MainLayout do it here. */}

      </Route> {/* End of Protected MainLayout routes */}

      {/* --- Catch-all Route --- */}
      {/* Matches any URL not previously defined. */}
      {/* Redirects based on authentication status: "/" if logged in, "/login" if not. */}
      <Route
        path="*"
        element={<Navigate to={token ? "/" : "/login"} replace />} // 'replace' avoids adding the redirect to browser history.
      />
    </Routes>
  );
}

// Exports the App component for rendering in main.tsx.
export default App;