import React from 'react';
// Link component for internal navigation without full page reloads.
import { Link } from 'react-router-dom';
// Importing the CSS for this Footer component.
import './Footer.css';

// The Footer functional component.
const Footer: React.FC = () => {
  return (
    // The main footer element.
    <footer className="footer">
      {/* Container for layout and centering of columns. */}
      <div className="footer-container">

        {/* Column 1: Logo & Social Media Icons */}
        <div className="footer-column footer-column-logo-social">
          {/* Displays the large logo. */}
          <img src="/assets/logo-large-white.png" alt="Borgertinget Logo" className="footer-logo" />
          {/* Container for social media links/icons. */}
          <div className="footer-social-icons">
            {/* Social media links using img tags for icons. aria-label improves accessibility. */}
            <a href="https://www.youtube.com/watch?v=dQw4w9WgXcQ" aria-label="External Link X"><img src="/assets/icons/x-icon.svg" alt="X Icon"/></a>
            <a href="https://www.youtube.com/watch?v=dQw4w9WgXcQ" aria-label="External Link Instagram"><img src="/assets/icons/instagram-icon.svg" alt="Instagram Icon"/></a>
            <a href="https://www.youtube.com/watch?v=dQw4w9WgXcQ" aria-label="External Link YouTube"><img src="/assets/icons/youtube-icon.svg" alt="YouTube Icon"/></a>
            <a href="https://www.youtube.com/watch?v=dQw4w9WgXcQ" aria-label="External Link LinkedIn"><img src="/assets/icons/linkedin-icon.svg" alt="LinkedIn Icon"/></a>
          </div>
        </div>

        {/* Column 2: Legal Links */}
        <div className="footer-column">
          <h4>Legal</h4> {/* Column title */}
          <ul> {/* List of legal links */}
            {/* Internal navigation using React Router's Link component. */}
            <li><Link to="https://www.youtube.com/watch?v=dQw4w9WgXcQ">Cookie Policy</Link></li>
            <li><Link to="https://www.youtube.com/watch?v=dQw4w9WgXcQ">Privacy Policy</Link></li>
            <li><Link to="https://www.youtube.com/watch?v=dQw4w9WgXcQ">GDPR</Link></li>
          </ul>
        </div>

        {/* Column 3: Company Links */}
        <div className="footer-column">
          <h4>Company</h4> {/* Column title */}
          <ul>
            <li><Link to="https://www.youtube.com/watch?v=dQw4w9WgXcQ">Contact Us</Link></li>
            <li><Link to="https://www.youtube.com/watch?v=dQw4w9WgXcQ">FAQ</Link></li>
          </ul>
        </div>
      </div>

       {/* Bottom section of the footer for copyright information. */}
       <div className="footer-bottom">
         {/* Displays the current year dynamically using JavaScript's Date object. */}
         <p>© {new Date().getFullYear()} Borgertinget. All rights reserved.</p>
       </div>
    </footer>
  );
};

// Exports the Footer component for use elsewhere.
export default Footer;