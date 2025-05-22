import React from 'react';
import { Outlet } from 'react-router-dom';
import Navbar from '../components/Navbar/Navbar';
import Footer from '../components/Footer/Footer';

// Props for MainLayout, so we can pass the token to the Navbar component.
// This allows the Navbar to have access to the token and update it when needed. (In our Logout)
interface MainLayoutProps {
  setToken: (token: string | null) => void;
}

// The MainLayout functional component.
const MainLayout: React.FC<MainLayoutProps> = ({ setToken }) => {
  return (
    // React Fragment avoids adding an unnecessary div
    <>
      {/* Renders the Navbar at the top of pages using this layout. */}
      <Navbar setToken={setToken} />
      {/* 'main' tag represents the primary content area. */}
      {/* Inline style ensures the main area fills at least the viewport height minus Navbar/Footer height, pushing the footer down. */}
      <main style={{ minHeight: 'calc(100vh - 150px)' }}>
        {/* Renders the component corresponding to the current nested route. */}
        <Outlet />
      </main>
      {/* Renders the Footer at the bottom of pages using this layout. */}
      <Footer />
    </>
  );
};

// Exports the layout component for use in routing configuration (App.tsx).
export default MainLayout;