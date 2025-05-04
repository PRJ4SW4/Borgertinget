import { useState, useEffect } from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import Login from "./pages/Login";
import Home from "./pages/Home";
import LearningLayout from "./layouts/LearningEnvironment/LearningLayout";
import PageContent from "./components/LearningEnvironment/PageContent";
import FlashcardLayout from "./layouts/Flashcards/FlashcardLayout";
import CalendarView from "./components/Calendar/CalendarView";

// Admin pages
import CreateFlashcardCollection from "./components/AdminPages/AddFlashcardCollection";
import AdminPage from "./components/AdminPages/AdminPage"; // Import new layout
import AdminBruger from "./components/AdminPages/AdminBruger";
import AdminIndhold from "./components/AdminPages/AdminIndhold";
import AdminLearing from "./components/AdminPages/AdminLearing";
import AdminPolls from "./components/AdminPages/AdminPolls";
import EditFlashcardCollection from "./components/AdminPages/EditFlashcardCollection";
import EditQuotes from "./components/AdminPages/EditCitatMode";
import AddPoll from "./components/AdminPages/AddPolls";
import EditPoll from "./components/AdminPages/EditPoll";
import DeletePoll from "./components/AdminPages/DeletePoll";
import AddLearningPage from "./components/AdminPages/AddLearningPage";
import EditLearningPage from "./components/AdminPages/EditLearningPage";
import DeleteLearningPage from "./components/AdminPages/DeleteLearningPage";

import LoginSuccessPage from "./pages/LoginSuccessPage";
import PartyPage from "./pages/PartyPage";
import PoliticianPage from "./pages/PoliticianPage";
import PartiesPage from "./pages/PartiesPage";
import FeedPage from "./pages/FeedPage"; // Tilføj denne linje

import Polidle from "./pages/Polidle/Polidle";
// Importet gamemodes
import ClassicMode from "./pages/Polidle/ClassicMode";
import CitatMode from "./pages/Polidle/CitatMode";
import FotoBlurMode from "./pages/Polidle/FotoBlurMode";
import EditCitatMode from "./components/AdminPages/EditCitatMode";

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
      <Route path="/feed" element={token ? <FeedPage /> : <Navigate to="/login" />} /> // Vis FeedPage hvis logget ind, ellers login
      <Route
          path="/learning"
          // Apply the SAME protection logic as /home
          element={token ? <LearningLayout /> : <Navigate to="/login" />}
        >
          <Route index element={<p>Velkommen til læringsområdet!</p>} />
          <Route path=":pageId" element={<PageContent />} />
      </Route>
      <Route path="/parties" element={<PartiesPage />} />
      <Route path="/party/:partyName" element={<PartyPage />} />
      <Route path="/politician/:id" element={<PoliticianPage />} />
      <Route
        path="/flashcards/*" // Match base path and potential nested paths
        element={token ? <FlashcardLayout /> : <Navigate to="/login" />}
      />
      <Route path="/login-success" element={<LoginSuccessPage setToken={setToken} />} />
      <Route path="*" element={<Navigate to={token ? "/home" : "/login"} />} />
      {/* Admin routes */}
      <Route path="/admin/*" element={token ? <AdminPage /> : <Navigate to="/home" />} />
      <Route path="/admin/Bruger" element={token ? <AdminBruger /> : <Navigate to="/home" />} />
      <Route path="/admin/Indhold" element={token ? <AdminIndhold /> : <Navigate to="/home" />} />
      <Route path="/admin/Laering" element={token ? <AdminLearing /> : <Navigate to="/home" />} />
      <Route path="/admin/Polls" element={token ? <AdminPolls /> : <Navigate to="/home" />} />
      <Route path="/admin/Polls/addPoll" element={token ? <AddPoll /> : <Navigate to="/home" />} />
      <Route path="/admin/Polls/editPoll" element={token ? <EditPoll /> : <Navigate to="/home" />} />
      <Route path="/admin/Polls/deletePoll" element={token ? <DeletePoll /> : <Navigate to="/home" />} />
      <Route path="/admin/Laering/addflashcardcollection" element={token ? <CreateFlashcardCollection /> : <Navigate to="/home" />} />
      <Route path="/admin/Laering/editflashcardcollection" element={token ? <EditFlashcardCollection /> : <Navigate to="/home" />} />
      <Route path="/admin/Laering/editcitatmode" element={token ? <EditQuotes /> : <Navigate to="/home" />} />
      <Route path="/admin/Laering/addLearningPage" element={token ? <AddLearningPage /> : <Navigate to="/home" />} />
      <Route path="/admin/Laering/editLearningPage" element={token ? <EditLearningPage /> : <Navigate to="/home" />} />
      <Route path="/admin/Laering/deleteLearningPage" element={token ? <DeleteLearningPage /> : <Navigate to="/home" />} />
      {/* Game Modes */}
      <Route path="/Polidle" element={<Polidle />} />
      <Route path="/ClassicMode" element={<ClassicMode />} />
      <Route path="/CitatMode" element={<CitatMode citat="Sample Citat" correctPolitiker="Sample Politician" />} />
      <Route path="/FotoBlurMode" element={<FotoBlurMode imageUrl="sample-image-url.jpg" correctPolitiker="Sample Politician" />} />
    </Routes>
  );
}

export default App;
