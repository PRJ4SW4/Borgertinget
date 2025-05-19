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
  const originalReturnUrl = searchParams.get('originalReturnUrl'); 

  if (token) {
    console.log("LoginSuccessPage: Modtaget token:", token);
    localStorage.setItem('jwt', token);
    setToken(token);

    const navigateTo = originalReturnUrl && originalReturnUrl.startsWith("/") ? originalReturnUrl : '/HomePage'; // Brug originalReturnUrl hvis den er valid, ellers default
    console.log("LoginSuccessPage: Navigerer til:", navigateTo);
    navigate(navigateTo, { replace: true });
  } else {
    console.error("LoginSuccessPage: Intet token fundet i URL.");
    navigate('/login', { replace: true });
  }
}, [location, navigate, setToken]); // Dependency array sikrer, at effekten kører igen hvis disse ændres 

  return (
    <div>
      <p>Logger ind via Google...</p>
    </div>
  );
};

export default LoginSuccessPage;