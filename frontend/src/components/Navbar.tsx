import React from 'react';
import { Link, NavLink } from 'react-router-dom';
import './Navbar.css'; 
import logo from "../images/Icon.png"; //Change to correct image (without text)

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
        //<header> for horizontal bar
        <header className="horizontalNavbar">
             <div className="navbarLogo"> {/* logo area */}
                <Link to="/home">
                    <img src={logo} alt="Borgertinget" /> {/* Use imported src and add alt text */}
                </Link>
             </div>
            <nav> {/* Wrap links in a nav element */}
                <ul> {/* Keep the list for structure */}
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
        </header>
    );
};

export default Navbar;