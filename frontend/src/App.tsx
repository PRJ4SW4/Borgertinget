import { useState, useEffect, JSX } from "react";
import { Routes, Route, Navigate } from "react-router-dom";

// Layout Components: Provide consistent page structure.
import LearningLayout from "./layouts/LearningEnvironment/LearningLayout";
import FlashcardLayout from "./layouts/Flashcards/FlashcardLayout";
import MainLayout from "./layouts/MainLayout"; // Standard layout with Navbar/Footer
import NavbarLandingPageLayout from "./layouts/LandingPage/NavbarLandingPageLayout"; // Different layout for the landing page.

// Page Components: Represent different views/pages in the application.
import Login from "./pages/Login";
import HomePage from "./pages/HomePage/HomePage";
import PageContent from "./components/LearningEnvironment/PageContent"; // Renders content within LearningLayout.
import CalendarView from "./components/Calendar/CalendarView";
import PartyPage from "./pages/PartyPage"; // Displays details for a specific party.
import PoliticianPage from "./pages/PoliticianPage"; // Displays details for a specific politician.
import PartiesPage from "./pages/PartiesPage"; // Displays a list of parties.
import LandingPage from "./pages/LandingPage/LandingPage";
import FeedPage from "./pages/FeedPage";

//* Polidle Pages
import PolidlePage from "./pages/PolidlePage/PolidlePage";
import ClassicMode from "./pages/PolidlePage/ClassicMode/ClassicMode";
import QuoteMode from "./pages/PolidlePage/QuoteMode/QuoteMode";
import FotoBlurMode from "./pages/PolidlePage/FotoBlurMode/FotoBlurMode";

import EmailVerification from "./utils/useEmailVerification"; // Handles email verification logic.
import LoginSuccessPage from "./pages/LoginSuccessPage"; // Handles post-login redirection.
// Admin Pages
import CreateFlashcardCollection from "./components/AdminPages/AddFlashcardCollection";
import AdminPage from "./components/AdminPages/AdminPage";
import AdminBruger from "./components/AdminPages/AdminBruger";
import AdminIndhold from "./components/AdminPages/AdminIndhold";
import RedigerIndhold from "./components/AdminPages/RedigerIndhold";
import AddEvent from "./components/AdminPages/AddEvent";
import EditEvent from "./components/AdminPages/EditEvent";
import DeleteEvent from "./components/AdminPages/DeleteEvent";
import AdminLearing from "./components/AdminPages/AdminLearing";
import AdminPolls from "./components/AdminPages/AdminPolls";
import EditFlashcardCollection from "./components/AdminPages/EditFlashcardCollection";
import EditQuotes from "./components/AdminPages/EditCitatMode";
import AddPoll from "./components/AdminPages/AddPolls";
import EditPoll from "./components/AdminPages/EditPoll";
import DeletePoll from "./components/AdminPages/DeletePoll";
import DeleteFlashcardCollection from "./components/AdminPages/DeleteFlashcardCollection";
import AddLearningPage from "./components/AdminPages/AddLearningPage";
import EditLearningPage from "./components/AdminPages/EditLearningPage";
import DeleteLearningPage from "./components/AdminPages/DeleteLearningPage";

import ResetPasswordVerification from "./utils/resetPassword";
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
      {/* --- Public Navbar Route --- */}
      <Route element={<NavbarLandingPageLayout />}>
        {" "}
        <Route path="/LandingPage" element={<LandingPage />} />
      </Route>{" "}
      {/* <<< Closed </Route> here */}
      {/* --- Public Routes (No MainLayout, No login required) --- */}
      {/* Login page route. */}
      <Route path="/login" element={<Login setToken={handleSetToken} />} />
      {/* Post-login success/callback route. */}
      <Route path="/login-success" element={<LoginSuccessPage setToken={handleSetToken} />} />
      <Route
        path="/verify"
        element={
          <EmailVerification
            onVerified={() => {}}
            onError={(message) => {
              console.error("Email verification error:", message);
            }}
          />
        }
      />
      <Route path="/reset-password" element={<ResetPasswordVerification />} />
      {/* --- Protected Routes using MainLayout --- */}
      {/* This Route group uses MainLayout and requires authentication via ProtectedRoute. */}
      {/* All nested routes inherit the layout and protection. */}
      <Route
        element={
          <ProtectedRoute>
            <MainLayout />
          </ProtectedRoute>
        }>
        {/* Root path ("/") route, shows HomePage for logged-in users. */}
        {/* Standard HomePage after login */}
        <Route path="/homepage" element={<HomePage />} />
        {/* Root path ("/") route, shows HomePage for logged-in users. */}
        {/* Calendar route (requires login). */}
        <Route path="/kalender" element={<CalendarView />} />
        <Route path="/feed" element={<FeedPage />} /> {/* Other routes requiring login and using MainLayout. */}
        <Route path="/parties" element={<PartiesPage />} />
        {/* ':partyName' is a dynamic URL parameter. */}
        <Route path="/party/:partyName" element={<PartyPage />} />
        {/* ':id' is a dynamic URL parameter. */}
        <Route path="/politician/:id" element={<PoliticianPage />} />
        {/* --- Learning Environment Routes --- */}
        {/* This route group uses LearningLayout and is protected by the parent ProtectedRoute. */}
        {/*Uses its own layout in addition to MainLayout, protection inherited.*/}
        <Route path="/learning" element={<LearningLayout />}>
          {" "}
          {/* Default content shown at "/learning". */}
          <Route index element={<p>Velkommen til læringsområdet! Vælg et emne i menuen.</p>} />
          {/* Route for specific learning pages, e.g., "/learning/topic-1". */}
          <Route path=":pageId" element={<PageContent />} />
          {/* ':pageId' is a dynamic URL parameter. */}
        </Route>
        {/* --- Flashcards Routes (Nested and Protected) --- */}
        {/* Uses FlashcardLayout, protection inherited. "/*" enables nested routing within
        {/* --- Flashcards Routes --- */}
        <Route path="/flashcards/*" element={<FlashcardLayout />} />
        {/* --- START: Polidle Routes (Beskyttet & i MainLayout) --- */}
        <Route path="/polidle" element={<PolidlePage />} />
        <Route path="/ClassicMode" element={<ClassicMode />} />
        <Route path="/CitatMode" element={<QuoteMode />} /> <Route path="/FotoBlurMode" element={<FotoBlurMode />} />{" "}
        {/* --- END: Polidle Routes --- */}
        {/* Admin routes */}
        <Route path="/admin/*" element={token ? <AdminPage /> : <Navigate to="/home" />} />
        <Route path="/admin/Bruger" element={token ? <AdminBruger /> : <Navigate to="/home" />} />
        <Route path="/admin/Indhold" element={token ? <AdminIndhold /> : <Navigate to="/home" />} />
        <Route path="/admin/Indhold/redigerIndhold" element={token ? <RedigerIndhold /> : <Navigate to="/home" />} />
        <Route path="/admin/Indhold/tilføjBegivenhed" element={token ? <AddEvent /> : <Navigate to="/home" />} />
        <Route path="/admin/Indhold/redigerBegivenhed" element={token ? <EditEvent /> : <Navigate to="/home" />} />
        <Route path="/admin/Indhold/sletBegivenhed" element={token ? <DeleteEvent /> : <Navigate to="/home" />} />
        <Route path="/admin/Laering" element={token ? <AdminLearing /> : <Navigate to="/home" />} />
        <Route path="/admin/Polls" element={token ? <AdminPolls /> : <Navigate to="/home" />} />
        <Route path="/admin/Polls/addPoll" element={token ? <AddPoll /> : <Navigate to="/home" />} />
        <Route path="/admin/Polls/editPoll" element={token ? <EditPoll /> : <Navigate to="/home" />} />
        <Route path="/admin/Polls/deletePoll" element={token ? <DeletePoll /> : <Navigate to="/home" />} />
        <Route path="/admin/Laering/addflashcardcollection" element={token ? <CreateFlashcardCollection /> : <Navigate to="/home" />} />
        <Route path="/admin/Laering/editflashcardcollection" element={token ? <EditFlashcardCollection /> : <Navigate to="/home" />} />
        <Route path="/admin/Laering/editcitatmode" element={token ? <EditQuotes /> : <Navigate to="/home" />} />
        <Route path="/admin/Laering/deleteFlashcardCollection" element={token ? <DeleteFlashcardCollection /> : <Navigate to="/home" />} />
        <Route path="/admin/Laering/addLearningPage" element={token ? <AddLearningPage /> : <Navigate to="/home" />} />
        <Route path="/admin/Laering/editLearningPage" element={token ? <EditLearningPage /> : <Navigate to="/home" />} />
        <Route path="/admin/Laering/deleteLearningPage" element={token ? <DeleteLearningPage /> : <Navigate to="/home" />} />{" "}
      </Route>{" "}
      {/* End of Protected MainLayout routes */}
      {/* Matches any URL not previously defined. */}
      {/* Redirects based on authentication status: "/" if logged in, "/login" if not. */}
      <Route path="/" element={<Navigate to={token ? "/homepage" : "/landingpage"} replace />} />
      {/* Catch-all for unkown roots: if logged in -> /homepage, or -> /landingpage */}
      <Route path="*" element={<Navigate to={token ? "/homepage" : "/landingpage"} replace />} />
    </Routes>
  );
}
// Exports the App component for rendering in main.tsx.
export default App;
