// src/components/Navbar/Navbar.tsx (eller hvor din Navbar.tsx fil ligger)
import React from "react";
import { NavLink, useNavigate } from "react-router-dom";
import logoSmall from "../../assets/logo-small.png"; // Sørg for at stien er korrekt
import "./Navbar.css"; // Sørg for at stien er korrekt

interface NavbarProps {
  setToken?: (token: string | null) => void;
}

const Navbar: React.FC<NavbarProps> = ({ setToken }) => {
  const navigate = useNavigate();

  const handleLogout = () => {
    console.log("Logging out from Navbar...");
    localStorage.removeItem("jwt");
    setToken?.(null);
    navigate("/landingpage"); // Eller '/login' hvis det er mere passende
  };

  return (
    <nav className="navbar">
      <div className="navbar-container">
        {/* Logo linker til forsiden for indloggede brugere (eller landing page hvis ikke logget ind) */}
        {/* Du har <NavLink to="/">, som i App.tsx redirecter til /homepage hvis logget ind */}
        <NavLink to="/" className="navbar-logo">
          <img src={logoSmall} alt="Borgertinget Logo" />
        </NavLink>

        <ul className="navbar-links">
          {/* Eksisterende links */}
          <li>
            <NavLink
              to="/parties"
              className={({ isActive }) =>
                isActive ? "nav-link active" : "nav-link"
              }
            >
              Politiske Sider
            </NavLink>
          </li>
          <li>
            <NavLink
              to="/feed"
              className={({ isActive }) =>
                isActive ? "nav-link active" : "nav-link"
              }
            >
              Feed
            </NavLink>
          </li>
          <li>
            <NavLink
              to="/kalender"
              className={({ isActive }) =>
                isActive ? "nav-link active" : "nav-link"
              }
            >
              Kalender
            </NavLink>
          </li>
          <li>
            <NavLink
              to="/learning"
              className={({ isActive }) =>
                isActive ? "nav-link active" : "nav-link"
              }
            >
              Læringsområde
            </NavLink>
          </li>
          <li>
            <NavLink
              to="/flashcards"
              className={({ isActive }) =>
                isActive ? "nav-link active" : "nav-link"
              }
            >
              Flashcards
            </NavLink>
          </li>

          <li>
            <NavLink
              to="/polidle" // Stien til din PolidlePage hub
              className={({ isActive }) =>
                isActive ? "nav-link active" : "nav-link"
              }
            >
              Polidle
            </NavLink>
          </li>

          <li className="logout-item">
            <button className="navbar-logout-button" onClick={handleLogout}>
              Log Ud
            </button>
          </li>
        </ul>
      </div>
    </nav>
  );
};

export default Navbar;
