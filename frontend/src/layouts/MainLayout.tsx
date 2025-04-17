import React from 'react';
import { Outlet } from 'react-router-dom';
import Navbar from '../components/Navbar.tsx';
import './MainLayout.css'; 

interface MainLayoutProps {
    token: string | null;
    setToken: (token: string | null) => void;
}

const MainLayout: React.FC<MainLayoutProps> = ({ token, setToken }) => {
    return (
        <div className="main-layout-container"> {/* container for Flexbox/Grid */}
            <Navbar token={token} setToken={setToken} /> {/* Navbar component */}
            <main className="main-content"> {/* Content area */}
                <Outlet /> {/* Page content*/}
            </main>
        </div>
    );
};

export default MainLayout;