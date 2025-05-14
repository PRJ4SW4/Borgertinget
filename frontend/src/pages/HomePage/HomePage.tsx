import React from 'react';
import { Link } from 'react-router-dom';
import './HomePage.css'; // Styles for HomePage layout
import SearchBar from '../../components/Searchbar'; // Import the new SearchBar component

// Placeholder for SearchDocument type, ensure it's defined or imported if needed by HomePage directly
// interface SearchDocument { ... } 

const HomePage: React.FC = () => {
  // All search-related state and logic has been moved to SearchBar.tsx

  return (
    <div className="homepage">
      <section className="hero-section">
        <div className="hero-content">
          <img src="/assets/logo-large-white.png" alt="Borgertinget Stort Logo" className="hero-logo" />
          <h1 className="hero-title">Borgertinget</h1>
          <p className="hero-subtitle">Din stemme, din viden, din fremtid</p>

          <div className="hero-search-container">
            <SearchBar />
          </div>

          <p className="hero-prompt">
            Ikke sikker på, hvor du skal starte?{' '}
            <a href="#features" className="hero-prompt-link">
              Udforsk Danmark's politiske læringsplatform nedenfor
            </a>
          </p>
          <a href="#features" className="hero-scroll-down" aria-label="Scroll down">
            ↓
          </a>
        </div>
      </section>

      <div id="features" className="features-container">
        <section className="feature-section">
          <div className="feature-text">
            <h2>Politik 101</h2>
            <p>En introduktion til politik i Danmark</p>
            <div className="feature-buttons">
                <Link to="/learning/1" className="feature-button-link"><button className="feature-button">Læs Politik 101</button></Link>
                <Link to="/flashcards/1" className="feature-button-link"><button className="feature-button">Øv med Flashcards</button></Link>
            </div>
          </div>
          <div className="feature-image">
            <img src="/assets/images/verdipolitik.png" alt="Værdipolitisk akse" />
          </div>
        </section>

        <section className="feature-section alt-layout">
          <div className="feature-text">
            <h2>Partierne & Politikerne</h2>
            <p>En oversigt over partierne og deres politikere</p>
            <div className="feature-buttons">
                <Link to="/parties" className="feature-button-link"><button className="feature-button">Partier</button></Link>
                <Link to="/politicians" className="feature-button-link"><button className="feature-button">Politikere</button></Link>
            </div>
          </div>
          <div className="feature-image">
             <img src="/assets/images/parti-logos.png" alt="Danske partilogoer" />
          </div>
        </section>

         <section className="feature-section">
           <div className="feature-text">
             <h2>Ugentlige Poldies</h2>
             <p>Sjovt minispil der udfordrer ens paratviden i politik</p>
             <div className="feature-buttons">
               <Link to="/polidles/classic" className="feature-button-link"><button className="feature-button">Klassisk</button></Link>
               <Link to="/polidles/image" className="feature-button-link"><button className="feature-button">Billede</button></Link>
             </div>
           </div>
           <div className="feature-image">
             <img src="/assets/images/polidles-game.png" alt="Polidles minispil eksempel" />
           </div>
         </section>
      </div>
    </div>
  );
};

export default HomePage;
