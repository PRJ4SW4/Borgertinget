import { useState, useEffect, JSX } from "react";
import { Routes, Route, Navigate } from "react-router-dom";

import LearningLayout from "./layouts/LearningEnvironment/LearningLayout";
import FlashcardLayout from "./layouts/Flashcards/FlashcardLayout";
import MainLayout from "./layouts/MainLayout";

import Login from "./pages/Login";
import Home from "./pages/Home";
import HomePage from "./pages/HomePage/HomePage";
import PageContent from "./components/LearningEnvironment/PageContent";
import CalendarView from "./components/Calendar/CalendarView";
import LoginSuccessPage from "./pages/LoginSuccessPage";
import PartyPage from "./pages/PartyPage";
import PoliticianPage from "./pages/PoliticianPage";
import PartiesPage from "./pages/PartiesPage";

// Admin Pages
import CreateFlashcardCollection from "./components/AdminPages/AddFlashcardCollection";
import AdminPage from "./components/AdminPages/AdminPage";
import AdminBruger from "./components/AdminPages/AdminBruger";
import AdminIndhold from "./components/AdminPages/AdminIndhold";
import RedigerIndhold from "./components/AdminPages/RedigerIndhold";
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

import Polidle from "./pages/Polidle/Polidle";
import ClassicMode from "./pages/Polidle/ClassicMode";
import CitatMode from "./pages/Polidle/CitatMode";
import FotoBlurMode from "./pages/Polidle/FotoBlurMode";

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
      <Route path="/login-success" element={<LoginSuccessPage setToken={setToken} />} />
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
        <Route path="/" element={<HomePage />} />
        {/* Home is an old route for a previous homepage, should be removed for production environment */}
        <Route
          path="/home"
          element={<Home setToken={setToken} />} // Pass setToken for logout functionality within Home.
        />
        {/* Calendar route (requires login). */}
        <Route path="/kalender" element={<CalendarView />} />
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
        <Route path="/flashcards/*" element={<FlashcardLayout />} />
        {/* If others need to define other protected routes using MainLayout do it here. */}
        {/* Admin routes */}
        <Route path="/admin/*" element={token ? <AdminPage /> : <Navigate to="/home" />} />
        <Route path="/admin/Bruger" element={token ? <AdminBruger /> : <Navigate to="/home" />} />
        <Route path="/admin/Indhold" element={token ? <AdminIndhold /> : <Navigate to="/home" />} />
        <Route path="/admin/Indhold/redigerIndhold" element={token ? <RedigerIndhold /> : <Navigate to="/home" />} />
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
      {/* --- Catch-all Route --- */}
      {/* Matches any URL not previously defined. */}
      {/* Redirects based on authentication status: "/" if logged in, "/login" if not. */}
      <Route
        path="*"
        element={<Navigate to={token ? "/" : "/login"} replace />} // 'replace' avoids adding the redirect to browser history.
      />
      {/* Game Modes */}
      <Route path="/Polidle" element={<Polidle />} />
      <Route path="/ClassicMode" element={<ClassicMode />} />
      <Route path="/CitatMode" element={<CitatMode citat="Sample Citat" correctPolitiker="Sample Politician" />} />
      <Route path="/FotoBlurMode" element={<FotoBlurMode imageUrl="sample-image-url.jpg" correctPolitiker="Sample Politician" />} />
    </Routes>
  );
}

// Exports the App component for rendering in main.tsx.
export default App;
