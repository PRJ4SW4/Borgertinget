import React from 'react';
// NavLink adds styling attributes for active routes.
// useNavigate provides a function for programmatic navigation.
import { NavLink, useNavigate } from 'react-router-dom';
// Importing the small logo image.
import logoSmall from '../../assets/logo-small.png';
// Importing the specific CSS for this component.
import './NavbarLanding.css';

// Defines the props expected by the NavbarLanding component.
interface NavbarLandingProps {
  // setToken is a function passed down from parent component (e.g., App.tsx).
  // It updates the authentication token state in the parent component.
  setToken?: (token: string | null) => void;
}

// The NavbarLanding functional component, receiving props defined by NavbarLandingProps.
const NavbarLanding: React.FC<NavbarLandingProps> = ({ setToken }) => {
  // Retrieves the navigate function from React Router for redirection.
  const navigate = useNavigate();

  // --- Logout Handler ---
  // Handles the click event on the logout button.
  const handleLogin = () => {
    // Logs the logout action for debugging.
    console.log("Moving to login from NavbarLandingPage...");
    // Removes the JWT token from the browser's local storage.
    // Calls the setToken function to update the application's authentication state to null.
    // Optional chaining () prevents errors if setToken is not passed.
    setToken?.(null);
    // Redirects the user to the login page after login button clicked.
    navigate('/login');
  };

  // The JSX structure of the NavbarLanding.
  return (
    <nav className="NavbarLanding"> {/* Main navigation element */}
      <div className="NavbarLanding-container"> {/* Container for layout */}
        {/* Logo links to the homepage */}
        <NavLink to="/" className="NavbarLanding-logo">
          <img src={logoSmall} alt="Borgertinget Logo" />
        </NavLink>

        {/* Navigation Links */}
        <ul className="NavbarLanding-links">
          {/* Standard navigation links using NavLink */}
          {/* The className function applies 'active' class based on route match */}

          {/* Logout Button */}
          {/* List item for structure */}
          <li className="login-item">
              {/* Button triggers the handleLogout function on click */}
              <button className="NavbarLanding-login-button" onClick={handleLogin}>
                 Log In / Opret Bruger
              </button>
          </li>
        </ul>
      </div>
    </nav>
  );
};

// Exports the component for use in other parts of the application.
export default NavbarLanding;