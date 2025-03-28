import React from "react";

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
      <button onClick={handleLogout}>Logout</button>
    </div>
  );
};

export default Home;
