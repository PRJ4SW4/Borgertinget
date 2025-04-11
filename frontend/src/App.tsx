import { useState, useEffect } from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/Login";
import Home from "./pages/Home";
import Verify from "./pages/Verify";
import LearningLayout from './layouts/LearningLayout';
import PageContent from './components/PageContent';
import FlashcardLayout from './layouts/FlashcardLayout'; // Import new layout
import AdminPage from './components/AdminPages/AdminPage'; // Import new layout
import AdminBruger from "./components/AdminPages/AdminBruger";
import AdminIndhold from "./components/AdminPages/AdminIndhold";
import AdminLearing from "./components/AdminPages/AdminLearing";
import AdminPolls from "./components/AdminPages/AdminPolls";

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
      {/* Admin routes */}
      <Route path="/admin/*" element={token ? <AdminPage /> : <Navigate to="/home" />} />
      <Route path="/admin/Bruger" element={token ? <AdminBruger /> : <Navigate to ="/home" />} />
      <Route path="/admin/Indhold" element={token ? <AdminIndhold /> : <Navigate to ="/home" />} />
      <Route path="/admin/Laering" element={token ? <AdminLearing /> : <Navigate to ="/home" />} />
      <Route path="/admin/Polls" element={token ? <AdminPolls /> : <Navigate to ="/home" />} />
    </Routes>
  );
}

export default App;
