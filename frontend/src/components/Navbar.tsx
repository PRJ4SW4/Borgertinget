import React from 'react';
import { Link, NavLink } from 'react-router-dom';
import './Navbar.css'; 
import logo from "../images/Icon.png";

interface NavbarProps {
    token: string | null;
    setToken: (token: string | null) => void;
}

const Navbar: React.FC<NavbarProps> = ({ token, setToken }) => {

    const handleLogout = () => {
        localStorage.removeItem("jwt");
        setToken(null);
    };

    return (
        // Use <aside> for semantic meaning of a sidebar
        <aside className="verticalNavbar">
             <div className="navbarLogo"> {/* logo area */}
                <Link to="/home">
                    <img src={logo} alt="Borgertinget" /> {/* Use imported src and add alt text */}
                </Link>
             </div>
            <nav> {/* Wrap links in a nav element */}
                <ul> {/* Keep the list for structure */}
                    <li><NavLink to="/home" className={({ isActive }) => isActive ? 'active-link' : ''}>Home</NavLink></li>
                    <li><NavLink to="/learning" className={({ isActive }) => isActive ? 'active-link' : ''}>Learning</NavLink></li>
                    <li><NavLink to="/flashcards" className={({ isActive }) => isActive ? 'active-link' : ''}>Flashcards</NavLink></li>
                    <li><NavLink to="/parties" className={({ isActive }) => isActive ? 'active-link' : ''}>Parties</NavLink></li>
                    <li><NavLink to="/kalender" className={({ isActive }) => isActive ? 'active-link' : ''}>Calendar</NavLink></li>
                    {/* Add other links */}
                </ul>
            </nav>
            {token && (
                 <div className="navbar-logout"> {/* Separate container for logout */}
                     <button onClick={handleLogout}>Logout</button>
                 </div>
            )}
        </aside>
    );
};

export default Navbar;