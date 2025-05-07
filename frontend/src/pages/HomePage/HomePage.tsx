import React from 'react';
// Importing the CSS for this HomePage component.
import './HomePage.css';

// The HomePage functional component, representing the main page after login.
const HomePage: React.FC = () => {
  return (
    // Main container for the homepage content.
    <div className="homepage">

      {/* --- Hero Section --- */}
      {/* The large introductory section at the top. */}
      <section className="hero-section">
        <div className="hero-content"> {/* Centers content */}
          {/* Large logo display. */}
          <img src="/assets/logo-large-white.png" alt="Borgertinget Stort Logo" className="hero-logo" />
          {/* Main headline. */}
          <h1 className="hero-title">Borgertinget</h1>
          {/* Subtitle/slogan. */}
          <p className="hero-subtitle">Din stemme, din viden, din fremtid</p>

          {/* Search Bar structure. */}
          <div className="hero-search-container">
            <input type="search" placeholder="Søg på tværs af Borgertinget" className="hero-search-input" />
          </div>

          {/* Text prompt guiding users. */}
          <p className="hero-prompt">
            Ikke sikker på, hvor du skal starte?{' '}
            {/* Link that scrolls the page down to the features section. */}
            <a href="#features" className="hero-prompt-link">
              Udforsk Danmark's politiske læringsplatform nedenfor
            </a>
          </p>

          {/* Animated arrow encouraging users to scroll. */}
          <a href="#features" className="hero-scroll-down" aria-label="Scroll down">
            ↓ {/* Down arrow character */}
          </a>
        </div>
      </section>

      {/* --- Feature Sections Container --- */}
      {/* Wraps all feature sections, targetable by the scroll link's ID. */}
      <div id="features" className="features-container">

        {/* Feature Section 1: Politik 101 */}
        {/* Highlights a key area of the platform. */}
        <section className="feature-section">
          {/* Text content for the feature. */}
          <div className="feature-text">
            <h2>Politik 101</h2>
            <p>En introduktion til politik i Danmark</p>
            {/* Buttons related to this feature. */}
            <div className="feature-buttons">
              <a href="/learning/1"><button className="feature-button">Læs Politik 101</button></a>
              <a href="/flashcards/1"><button className="feature-button">Øv med Flashcards</button></a>
            </div>
          </div>
          {/* Image representing the feature. */}
          <div className="feature-image">
            <img src="/assets/images/verdipolitik.png" alt="Værdipolitisk akse" />
          </div>
        </section>

        {/* Feature Section 2: Partierne & Politikerne */}
        {/* 'alt-layout' class modifies the visual order of text/image. */}
        <section className="feature-section alt-layout">
          <div className="feature-text">
            <h2>Partierne & Politikerne</h2>
            <p>En oversigt over partierne og deres politikere</p>
            <div className="feature-buttons">
              <a href="/parties"><button className="feature-button">Partier</button></a>
              <a href="/party/Dansk%20Folkeparti"><button className="feature-button">Politikere</button></a>
            </div>
          </div>
          <div className="feature-image">
             <img src="/assets/images/parti-logos.png" alt="Danske partilogoer" />
          </div>
        </section>

        {/* Feature Section 3: Ugentlige Poldies */}
        {/* Needs to be update by @FinkThePanda */}
         <section className="feature-section">
           <div className="feature-text">
             <h2>Ugentlige Poldies</h2>
             <p>Sjovt minispil der udfordrer ens paratviden i politik</p>
             <div className="feature-buttons">
               <a href="https://www.youtube.com/watch?v=dQw4w9WgXcQ"><button className="feature-button">Klassisk</button></a>
               <a href="https://www.youtube.com/watch?v=dQw4w9WgXcQ"><button className="feature-button">Billede</button></a>
             </div>
           </div>
           <div className="feature-image">
             <img src="/assets/images/polidles-game.png" alt="Polidles minispil eksempel" />
           </div>
         </section>

      </div> {/* End features-container */}
    </div> // End homepage div
  );
};

// Exports the HomePage component for routing.
export default HomePage;