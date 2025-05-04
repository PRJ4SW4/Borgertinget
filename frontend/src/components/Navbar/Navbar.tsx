import React from "react";
// NavLink adds styling attributes for active routes.
// useNavigate provides a function for programmatic navigation.
import { NavLink, useNavigate } from "react-router-dom";
// Importing the small logo image.
import logoSmall from "../../assets/logo-small.png";
// Importing the specific CSS for this component.
import "./Navbar.css";

// Defines the props expected by the Navbar component.
interface NavbarProps {
  // setToken is a function passed down from parent component (e.g., App.tsx).
  // It updates the authentication token state in the parent component.
  setToken?: (token: string | null) => void;
}

// The Navbar functional component, receiving props defined by NavbarProps.
const Navbar: React.FC<NavbarProps> = ({ setToken }) => {
  // Retrieves the navigate function from React Router for redirection.
  const navigate = useNavigate();

  // --- Logout Handler ---
  // Handles the click event on the logout button.
  const handleLogout = () => {
    // Logs the logout action for debugging.
    console.log("Logging out from Navbar...");
    // Removes the JWT token from the browser's local storage.
    localStorage.removeItem("jwt");
    // Calls the setToken function to update the application's authentication state to null.
    // Optional chaining () prevents errors if setToken is not passed.
    setToken?.(null);
    // Redirects the user to the login page after logout actions.
    navigate("/login");
  };

  // --- JWT Role Checking ---
  const token = localStorage.getItem("jwt");
  let isAdmin = false;

  if (token) {
    try {
      const payload = JSON.parse(atob(token.split(".")[1]));
      const roles = payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];
      isAdmin = Array.isArray(roles) ? roles.includes("Admin") : roles === "Admin";
    } catch (err) {
      console.error("Invalid token", err);
    }
  }

  // The JSX structure of the Navbar.
  return (
    <nav className="navbar">
      {" "}
      {/* Main navigation element */}
      <div className="navbar-container">
        {" "}
        {/* Container for layout */}
        {/* Logo links to the homepage */}
        <NavLink to="/" className="navbar-logo">
          <img src={logoSmall} alt="Borgertinget Logo" />
        </NavLink>
        {/* Navigation Links */}
        <ul className="navbar-links">
          {/* Conditionally render Admin link */}
          {isAdmin && (
            <li>
              <NavLink to="/admin" className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}>
                Admin
              </NavLink>
            </li>
          )}
          {/* Standard navigation links using NavLink */}
          {/* The className function applies 'active' class based on route match */}
          <li>
            <NavLink to="/parties" className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}>
              Politiske Sider
            </NavLink>
          </li>
          <li>
            <NavLink to="/feed" className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}>
              Feed
            </NavLink>
          </li>
          <li>
            <NavLink to="/kalender" className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}>
              Kalender
            </NavLink>
          </li>
          <li>
            <NavLink to="/learning" className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}>
              Læringsområde
            </NavLink>
          </li>
          <li>
            <NavLink to="/flashcards" className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}>
              Flashcards
            </NavLink>
          </li>

          {/* Logout Button */}
          {/* List item for structure */}
          <li className="logout-item">
            {/* Button triggers the handleLogout function on click */}
            <button className="navbar-logout-button" onClick={handleLogout}>
              Log Ud
            </button>
          </li>
        </ul>
      </div>
    </nav>
  );
};

// Exports the component for use in other parts of the application.
export default Navbar;
