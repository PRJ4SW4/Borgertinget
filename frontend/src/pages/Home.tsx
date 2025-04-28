import React from "react";
import { Link } from 'react-router-dom';

interface HomeProps {
  setToken: (token: string | null) => void;
}

const Home: React.FC<HomeProps> = ({ setToken }) => {
  const handleLogout = () => {
    localStorage.removeItem("jwt");
    setToken(null);
  };

  return (
    <div>
      <h2>Welcome to the Home Page</h2>
      <nav className="home-navigation">
        <ul>
          {/* Link to parties*/}
          <li>
            <Link to="/parties">Se Partier</Link>
          </li>
        </ul>
      </nav>
      <button onClick={handleLogout}>Logout</button>
    </div>
  );
};

export default Home;
