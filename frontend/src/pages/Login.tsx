import { useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import "./Login.css";
import loginImage from "../images/LoginImage.png";

interface LoginProps {
  setToken: (token: string | null) => void;
}

const Login: React.FC<LoginProps> = ({ setToken }) => {  
  const [loginUsername, setLoginUsername] = useState<string>("");
  const [loginPassword, setLoginPassword] = useState<string>("");
  const [registerUsername, setRegisterUsername] = useState<string>("");
  const [registerPassword, setRegisterPassword] = useState<string>("");
  const [registerEmail, setRegisterEmail] = useState<string>("");
  const [error, setError] = useState<string | null>("");
  const [success, setSuccess] = useState<string | null>("");
  //const [showRegister, setShowRegister] = useState(false);
  const [startSlide, setStartSlide] = useState(false);
  const navigate = useNavigate();

  const handleLogin = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);

    try {
      const response = await axios.post("http://localhost:5218/api/users/login", { 
        EmailOrUsername: loginUsername, 
        Password: loginPassword 
      });

      const token = response.data.token;
      localStorage.setItem("jwt", token);
      setToken(token);
      navigate("/home");
    }
    
    catch (error) {
      if (axios.isAxiosError(error)) {
          const errorMessage = error.response?.data?.error || "Noget gik galt. Prøv igen.";
          setError(errorMessage); // Sikrer at der altid er en fejlmeddelelse
      } else {
          setError("Ingen forbindelse til serveren.");
      }
    }
  };

  const handleRegister = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setSuccess(null);
    setError(null);

    try {
        const response = await axios.post("http://localhost:5218/api/users/register", { 
        Username: registerUsername,
        Email: registerEmail, 
        Password: registerPassword
      });
      setSuccess(response.data.message);

    } catch (error) {
        if (axios.isAxiosError(error)) {
          if (error.response && error.response.data.error) {
              setError(error.response.data.error); // Fanger backend-fejlbesked
          } else {
              setError("Noget gik galt. Prøv igen.");
          }
      } else {
          setError("Ingen forbindelse til serveren.");
      }
    }
  };

  const handleGoogleLogin = () => {
    const googleAuthUrl = 'https://accounts.google.com/o/oauth2/v2/auth';

    const clientId = '16121037113-cioi5949kloah9uac0c4laqknumogptc.apps.googleusercontent.com';

    const redirectUri = 'http://localhost:5218/auth/google/callback'; // Din backend callback

    const options = {
      client_id: clientId,
      redirect_uri: redirectUri,
      response_type: 'code',
      scope: 'openid profile email', // Standard scopes til login
      // state: 'tilfaeldig-sikkerheds-streng' // Implementeres senere for CSRF-beskyttelse
    };

    const queryString = new URLSearchParams(options).toString();

    // Omdirigerer brugerens browser
    console.log("Redirecting to Google:", `${googleAuthUrl}?${queryString}`); // God til debugging
    window.location.href = `${googleAuthUrl}?${queryString}`;
  };

  return (
    <div className="outer-wrapper">
      <div className={`login-container ${startSlide ? "shifted" : ""}`}>
        
        {/* Login Form Section */}
        
        <div className="login-form">
        <h1>Velkommen til Borgertinget</h1>
          <div className="form-content">
            <p className="slogan">
              Din stemme <span className="ball"></span> Din viden <span className="ball"></span> Din fremtid
            </p>
  
            <form onSubmit={handleLogin} className="form-container">
              <div className="form-group">
                <p>Email/brugernavn</p>
                <input
                  className="input-field"
                  type="text"
                  placeholder="Indtast email eller brugernavn"
                  value={loginUsername}
                  onChange={(e) => setLoginUsername(e.target.value)}
                  required
                />
              </div>
  
              <div className="form-group">
                <p>Kodeord</p>
                <input
                  className="input-field"
                  type="password"
                  placeholder="Indtast kodeord"
                  value={loginPassword}
                  onChange={(e) => setLoginPassword(e.target.value)}
                  required
                />
                <div className="error-message">
                  <p>{error || "\u00A0"}</p>
                </div>
              </div>
  
              <button className="button" type="submit">Log på</button>
            </form>
  
            <p className="forgot-password">Glemt kodeord?</p>
            <p>Log på med</p>
            <button
              type="button"
              onClick={handleGoogleLogin} 
              className="google-login-button"
              aria-label="Log på med Google" 
            >
            </button>
            <p className="no-account">
              Har du ikke en konto? Opret en <span className="link-text" onClick={() => setStartSlide(true)}>her</span>
            </p>
          </div>
        </div>
  
        {/* Register Form Section */}
        <div className="register-form">
          <div className="form-content">
            <h1>Opret en konto</h1>
            {success && <p style={{ color: "green" }}>{success}</p>}
            <form onSubmit={handleRegister} className="form-container">
            <div className="form-group">
            <p>Brugernavn</p>
              <input
                className="input-field"
                type="text"
                placeholder="Indtast brugernavn"
                value={registerUsername}
                onChange={(e) => setRegisterUsername(e.target.value)}
                required
              />
              </div>
              <div className="form-group">
              <p>Email</p>
              <input
                className="input-field"
                type="email"
                placeholder="Indtast email"
                value={registerEmail}
                onChange={(e) => setRegisterEmail(e.target.value)}
                required
              />
              </div>
              <div className="form-group">
              <p>Kodeord</p>
              <input
                className="input-field"
                type="password"
                placeholder="Indtast kodeord"
                value={registerPassword}
                onChange={(e) => setRegisterPassword(e.target.value)}
                required
              />
              </div>
              <div className="error-message">
                  <p>{error || "\u00A0"}</p>
                </div>
              <button className="button" type="submit">Opret konto</button>
            </form>
  
            <p className="link-text" onClick={() => setStartSlide(false)}>
              Tilbage til login
            </p>
          </div>
        </div>
  
        {/* Image Section */}
        <div className="image-section">
          <img src={loginImage} className="login-image" alt="Login visual" />
        </div>
      </div>
    </div>
  );
  
  };

export default Login;
