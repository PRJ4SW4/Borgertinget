import React, { useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

interface LoginSuccessPageProps {
  setToken: (token: string | null) => void;
}

const LoginSuccessPage: React.FC<LoginSuccessPageProps> = ({ setToken }) => {
  const location = useLocation(); // Giver adgang til den aktuelle URL's info
  const navigate = useNavigate(); // Bruges til at navigere programmatisk

  useEffect(() => {
    const searchParams = new URLSearchParams(location.search);
    const token = searchParams.get('token'); 

    if (token) {
      console.log("Modtaget token fra Google login:", token); // Til debugging
      localStorage.setItem('jwt', token);
      setToken(token);
      navigate('/HomePage', { replace: true });
    } else {
      console.error("Intet token fundet i URL efter Google login.");
      navigate('/login'); // Eller vis en fejlside
    }
  }, [location, navigate, setToken]); // Dependency array sikrer, at effekten kører igen hvis disse ændres (selvom det næppe sker her)

  return (
    <div>
      <p>Logger ind via Google...</p>
    </div>
  );
};

export default LoginSuccessPage;