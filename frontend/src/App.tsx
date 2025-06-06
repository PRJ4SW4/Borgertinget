import { useState, useEffect, JSX, useCallback, useRef } from "react";
import { Routes, Route, Navigate, useNavigate, useLocation } from "react-router-dom";

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

// Utility function to check if a JWT token is expired.
const isTokenExpired = (token: string | null): boolean => {
  if (!token) return true;
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    const exp = payload.exp; // Expiration time in seconds since epoch
    return Date.now() >= exp * 1000; // Convert to milliseconds and compare
  } catch (err) {
    console.error("Invalid token", err);
    return true;
  }
};

// --- Protected Route Component ---
// Wraps routes that require user authentication.
// Renders the child component if a token exists, otherwise redirects to /login.

// ProtectedRoute HAS TO BE outside the App component
// Else this will cause pages to be re-rendered on every render of App, which is not good
// for things like SideNav, which depend on only being mounted once
const ProtectedRoute: React.FC<{ token: string | null; children: JSX.Element }> = ({ token, children }) => {
  return token ? children : <Navigate to="/login" />;
};

function App() {
  // State hook for the JWT authentication token.
  // Initializes state from localStorage to persist login status.
  const [token, setToken] = useState<string | null>(localStorage.getItem("jwt"));
  const [isAdmin, setIsAdmin] = useState<boolean>(false); // State for admin role

  // useCallback is used to memoize the function, which means the function reference will remain the same
  // across renders unless its inputs change.
  // This makes certain things that require to only be mounted once don't get re-mounteed
  const memoizedHandleSetToken = useCallback((newToken: string | null) => {
    setToken(newToken);
    if (newToken) {
      localStorage.setItem("jwt", newToken);
      try {
        const payload = JSON.parse(atob(newToken.split(".")[1]));
        const roles = payload["role"];
        setIsAdmin(Array.isArray(roles) ? roles.includes("Admin") : roles === "Admin");
      } catch (err) {
        console.error("Invalid token", err);
        setIsAdmin(false);
      }
    } else {
      localStorage.removeItem("jwt");
      setIsAdmin(false);
    }
  }, []); // setToken from useState is stable, so empty dependency array here, because we DONT
  // need to re-create the function on every render, that previously broke my SideNav ;(

  // Effect hook to synchronize token state with localStorage changes across tabs/windows.
  useEffect(() => {
    const handleStorageChange = () => {
      const storedToken = localStorage.getItem("jwt");
      setToken(storedToken);
      if (storedToken) {
        try {
          const payload = JSON.parse(atob(storedToken.split(".")[1]));
          const roles = payload["role"];
          setIsAdmin(Array.isArray(roles) ? roles.includes("Admin") : roles === "Admin");
        } catch (err) {
          console.error("Invalid token", err);
          setIsAdmin(false);
        }
      } else {
        setIsAdmin(false);
      }
    };

    // Adds the event listener on component mount.
    window.addEventListener("storage", handleStorageChange);
    // Removes the event listener on component unmount to prevent memory leaks.
    return () => window.removeEventListener("storage", handleStorageChange);
  }, []); // Empty dependency array ensures the effect runs only on mount and unmount.

  const navigate = useNavigate();
  const location = useLocation();
  const alertShownRef = useRef(false); // Prevent double alert on expired token

  useEffect(() => {
    // Check token expiration on route change or token change
    if (token && isTokenExpired(token)) {
      if (!alertShownRef.current) {
        alertShownRef.current = true;
        console.log("Token expired. Logging out...");
        memoizedHandleSetToken(null);
        navigate("/login", { replace: true }); // Redirect to login page without adding to history
        alert("Din session er udløbet. Log ind igen for at fortsætte."); // Alert user about session expiration
      }
    } else {
      alertShownRef.current = false; // Reset if token is valid or null
    }
  }, [token, location, memoizedHandleSetToken, navigate]); // Run effects on token or route change

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
      <Route path="/login" element={<Login setToken={memoizedHandleSetToken} />} />
      {/* Post-login success/callback route. */}
      <Route path="/login-success" element={<LoginSuccessPage setToken={memoizedHandleSetToken} />} />
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
          <ProtectedRoute token={token}>
            <MainLayout setToken={memoizedHandleSetToken} />
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
        <Route path="/admin/*" element={token && isAdmin ? <AdminPage /> : <Navigate to="/home" />} />
        <Route path="/admin/Bruger" element={token && isAdmin ? <AdminBruger /> : <Navigate to="/home" />} />
        <Route path="/admin/Indhold" element={token && isAdmin ? <AdminIndhold /> : <Navigate to="/home" />} />
        <Route path="/admin/Indhold/redigerIndhold" element={token && isAdmin ? <RedigerIndhold /> : <Navigate to="/home" />} />
        <Route path="/admin/Indhold/tilføjBegivenhed" element={token && isAdmin ? <AddEvent /> : <Navigate to="/home" />} />
        <Route path="/admin/Indhold/redigerBegivenhed" element={token && isAdmin ? <EditEvent /> : <Navigate to="/home" />} />
        <Route path="/admin/Indhold/sletBegivenhed" element={token && isAdmin ? <DeleteEvent /> : <Navigate to="/home" />} />
        <Route path="/admin/Laering" element={token && isAdmin ? <AdminLearing /> : <Navigate to="/home" />} />
        <Route path="/admin/Polls" element={token && isAdmin ? <AdminPolls /> : <Navigate to="/home" />} />
        <Route path="/admin/Polls/addPoll" element={token && isAdmin ? <AddPoll /> : <Navigate to="/home" />} />
        <Route path="/admin/Polls/editPoll" element={token && isAdmin ? <EditPoll /> : <Navigate to="/home" />} />
        <Route path="/admin/Polls/deletePoll" element={token && isAdmin ? <DeletePoll /> : <Navigate to="/home" />} />
        <Route path="/admin/Laering/addflashcardcollection" element={token && isAdmin ? <CreateFlashcardCollection /> : <Navigate to="/home" />} />
        <Route path="/admin/Laering/editflashcardcollection" element={token && isAdmin ? <EditFlashcardCollection /> : <Navigate to="/home" />} />
        <Route path="/admin/Laering/editcitatmode" element={token && isAdmin ? <EditQuotes /> : <Navigate to="/home" />} />
        <Route path="/admin/Laering/deleteFlashcardCollection" element={token && isAdmin ? <DeleteFlashcardCollection /> : <Navigate to="/home" />} />
        <Route path="/admin/Laering/addLearningPage" element={token && isAdmin ? <AddLearningPage /> : <Navigate to="/home" />} />
        <Route path="/admin/Laering/editLearningPage" element={token && isAdmin ? <EditLearningPage /> : <Navigate to="/home" />} />
        <Route path="/admin/Laering/deleteLearningPage" element={token && isAdmin ? <DeleteLearningPage /> : <Navigate to="/home" />} />{" "}
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
