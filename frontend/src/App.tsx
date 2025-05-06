// src/App.tsx
import { useState, useEffect, type ReactNode } from "react"; // Added ReactNode import
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

// Import the new layout
import MainLayout from './layouts/MainLayout';

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

    // Helper for protected routes using ReactNode to redirect to login and send them to where they were trying to access
    const ProtectedRoute = ({ children }: { children: ReactNode }) => {
        if (!token) {
            return <Navigate to="/login" replace />;
        }
        // `children` is returned directly, React handles rendering ReactNode
        return children; 
    };


    return (
        <Routes>
            {/* Routes WITHOUT the main navbar */}
            <Route path="/login" element={<Login setToken={setToken} />} />
            {/*<Route path="/verify" element={<Verify />} />*/}
            <Route path="/login-success" element={<LoginSuccessPage setToken={setToken} />} />


            {/* Routes WITH the main navbar */}
            {/* Use MainLayout as the element for this parent route */}
            {/* All nested routes will render inside MainLayout's <Outlet> */}
            <Route
                element={
                    <ProtectedRoute>
                        <MainLayout token={token} setToken={setToken} />
                    </ProtectedRoute>
                }
            >
                {/* These routes are now children of MainLayout */}
                {/* They require the user to be logged in because the parent route is protected */}
                <Route path="/home" element={<Home setToken={setToken} />} /> {/* Pass setToken if Home needs it */}
                <Route path="/kalender" element={<CalendarView />} />
                <Route path="/parties" element={<PartiesPage />} />
                <Route path="/party/:partyName" element={<PartyPage />} />
                <Route path="/politician/:id" element={<PoliticianPage />} />


                {/* Nested Layouts like LearningLayout still work! */}
                {/* LearningLayout will render inside MainLayout's <Outlet> */}
                <Route path="/learning" element={<LearningLayout />}>
                    <Route index element={<p>Velkommen til læringsområdet!</p>} />
                    <Route path=":pageId" element={<PageContent />} />
                </Route>

                 {/* FlashcardLayout will render inside MainLayout's <Outlet> */}
                <Route path="/flashcards/*" element={<FlashcardLayout />} />

                 {/* Add any other routes that should have the main navbar here */}

            </Route> {/* End of routes wrapped by MainLayout */}


            {/* Catch-all Route: Redirects based on token status */}
            {/* Place it last */}
            <Route path="*" element={<Navigate to={token ? "/home" : "/login"} replace />} />
      <Route path="/Polidle" element={<Polidle />} />
      // gamemodes
      <Route path="/ClassicMode" element={<ClassicMode />} />
      <Route
        path="/CitatMode"
        element={
          <CitatMode
            citat="Sample Citat"
            correctPolitiker="Sample Politician"
          />
        }
      />
      <Route
        path="/FotoBlurMode"
        element={
          <FotoBlurMode
            imageUrl="sample-image-url.jpg"
            correctPolitiker="Sample Politician"
          />
        }
      />

        </Routes>
    );
}

export default App;