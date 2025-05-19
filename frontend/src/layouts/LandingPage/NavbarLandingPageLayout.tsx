import React from 'react';
import { Outlet } from 'react-router-dom';
import NavbarLanding from '../../components/NavbarLanding/NavbarLanding.tsx';
import Footer from '../../components/Footer/Footer';

// The MainLayout functional component.
const MainLayout: React.FC = () => {
  return (
    // React Fragment avoids adding an unnecessary div
    <>
      {/* Renders the Navbar at the top of pages using this layout. */}
      <NavbarLanding />
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