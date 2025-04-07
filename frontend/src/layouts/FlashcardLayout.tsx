// src/layouts/FlashcardLayout.tsx
import { Routes, Route } from 'react-router-dom'; // Use Outlet or nested Routes
import FlashcardSideNav from '../components/FlashcardSideNav';
import FlashcardViewer from '../components/FlashcardViewer'; // Create this soon
import './FlashcardLayout.css';

function FlashcardLayout() {
    return (
      <div className="flashcard-layout">
        <aside className="flashcard-sidenav-container">
           <FlashcardSideNav />
        </aside>
        <main className="flashcard-content-area">
           {/* Define nested routes */}
           <Routes>
               {/* Route for the base /flashcards path */}
               <Route index element={<p>Vælg venligst en flashcard-samling fra menuen til venstre.</p>} />
               {/* Route that renders the viewer when a collectionId is present */}
               <Route path=":collectionId" element={<FlashcardViewer />} />
           </Routes>
        </main>
      </div>
    );
  }
  export default FlashcardLayout;