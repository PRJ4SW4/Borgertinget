import React, { useEffect } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';

interface LoginSuccessPageProps {
  setToken: (token: string | null) => void;
}

const LoginSuccessPage: React.FC<LoginSuccessPageProps> = ({ setToken }) => {
  const location = useLocation(); // Giver adgang til den aktuelle URL's info
  const navigate = useNavigate(); // Bruges til at navigere programmatisk

  useEffect(() => {
    // Kør denne logik én gang, når komponenten indlæses
    const searchParams = new URLSearchParams(location.search);
    const token = searchParams.get('token'); // Hent 'token' parameteret fra ?token=...

    if (token) {
      console.log("Modtaget token fra Google login:", token); // Til debugging
      // Gem token og opdater state, ligesom i din handleLogin
      localStorage.setItem('jwt', token);
      setToken(token);
      // Naviger til hjemmesiden (eller dashboard) og erstat /login-success i historikken
      navigate('/', { replace: true });
    } else {
      console.error("Intet token fundet i URL efter Google login.");
      // Håndter fejl - send evt. brugeren tilbage til login
      navigate('/login'); // Eller vis en fejlside
    }
  }, [location, navigate, setToken]); // Dependency array sikrer, at effekten kører igen hvis disse ændres (selvom det næppe sker her)

  // Vis en simpel loading-besked, mens token behandles
  return (
    <div>
      <p>Logger ind via Google...</p>
    </div>
  );
};

export default LoginSuccessPage;