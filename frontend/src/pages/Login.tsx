import { useState, useEffect, useCallback } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import axios from "axios";
import styles from "./Login.module.css";
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
  const [statusMessage, setStatusMessage] = useState<string | null>("");
  const [statusHeader, setStatusHeader] = useState<string | null>("Fejl");
  const [showPopup, setShowPopup] = useState(false);
  const [showForgotPassword, setShowForgotPassword] = useState(false);
  const [forgotPasswordEmail, setForgotPasswordEmail] = useState<string>("");
  const [startSlide, setStartSlide] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  const handleVerificationMessage = useCallback(() => {
    const params = new URLSearchParams(location.search);
    const message = params.get("message");
    const status = params.get("status");
    if (message && status) {
      setStatusMessage(message);
      setStatusHeader(status === "success" ? "Succes" : "Fejl");
      setShowPopup(true);
      window.history.replaceState({}, document.title, window.location.pathname);
    }
  }, [location.search]);
  useEffect(() => {
    handleVerificationMessage();
  }, [handleVerificationMessage]); // Call it in the useEffect

  const handleLogin = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setStatusHeader("Fejl");
    setStatusMessage(null);
    try {
      const response = await axios.post("http://localhost:5218/api/users/login", { 
        EmailOrUsername: loginUsername, 
        Password: loginPassword 
      });

      const tokenString = JSON.stringify(response.data.token);
      localStorage.setItem("jwt", tokenString);
      setToken(tokenString);
      navigate("/");
    }
    
    catch (error) {
      if (axios.isAxiosError(error)) {
          const errorMessage = error.response?.data?.error || "Noget gik galt. Prøv igen.";
          setStatusMessage(errorMessage); // Sikrer at der altid er en fejlmeddelelse
          setShowPopup(true);
      } else {
          setStatusMessage("Ingen forbindelse til serveren.");
          setShowPopup(true);
      }
    }
  };

  const handleRegister = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setStatusMessage(null);
    setStatusHeader("Fejl");

    try {
        const response = await axios.post("http://localhost:5218/api/users/register", { 
        Username: registerUsername,
        Email: registerEmail, 
        Password: registerPassword
      });
      setStatusMessage(response.data.message);
      setStatusHeader("Succes");
      setShowPopup(true);

    } 
    catch (error) {
      if (axios.isAxiosError(error)) {
          const responseData = error.response?.data;
          console.log("Response Data:", responseData); // Log responseData
          console.log("Error:", error); // Log error
          if (responseData?.error) {
            setStatusMessage(responseData.error); // Fanger backend-fejlbesked
            setShowPopup(true);
          } else if (responseData?.errors) {
            if (Array.isArray(responseData.errors)) {
              setStatusMessage(responseData.errors[0]); // Fanger den første fejlmeddelelse
              setShowPopup(true);
            } else if (typeof responseData.errors === "object") {
              const firstKey = Object.keys(responseData.errors)[0];
              const firstError = responseData.errors[firstKey][0]; // Fanger den første fejlmeddelelse

              setStatusMessage(firstError); // Fanger backend-fejlbesked
              setShowPopup(true);
            } else {
            console.log("Unexpected Axios error shape:", error.response?.data);
            setStatusMessage("Noget gik galt. Prøv igen.");
            setShowPopup(true);
          }
        } else {
          console.log("No connection or unknown error:", error);
          setStatusMessage("Ingen forbindelse til serveren.");
          setShowPopup(true);
        }
      }  
    }
  };

  const handleForgotPassword = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    try {
      const response = await axios.post("http://localhost:5218/api/users/forgot-password", { 
        Email: forgotPasswordEmail
      });
      setStatusMessage(response.data.message);
      setStatusHeader("Succes");
      setShowPopup(true);
    } catch (error) {
      if (axios.isAxiosError(error)) {
        const errorMessage = error.response?.data?.error || "Noget gik galt. Prøv igen.";
        setStatusHeader("Fejl");
        setStatusMessage(errorMessage);
        setShowPopup(true);
      } else {
        setStatusHeader("Fejl");
        setStatusMessage("Ingen forbindelse til serveren.");
        setShowPopup(true);
      }
    } finally {
        setShowForgotPassword(false);
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
      scope: 'openid profile email', 
      // state: 'tilfaeldig-sikkerheds-streng' // Implementeres senere for CSRF-beskyttelse
    };

    const queryString = new URLSearchParams(options).toString();

    console.log("Redirecting to Google:", `${googleAuthUrl}?${queryString}`); 
    window.location.href = `${googleAuthUrl}?${queryString}`;
  };

  return (
    <div className={styles.outerWrapper}>
      <div className={`${styles.loginContainer} ${startSlide ? styles.shifted : ""}`}>
        
        {/* Login Form Section */}
        
        <div className={styles.loginForm}>
        <h1>Velkommen til Borgertinget</h1>
          <div className={styles.formContent}>
            <p className={styles.slogan}>
              Din stemme <span className={styles.ball}></span> Din viden <span className={styles.ball}></span> Din fremtid
            </p>
  
            <form onSubmit={handleLogin} className={styles.formContainer}>
              <div className={styles.formGroup}>
                <p>Email/brugernavn</p>
                <input
                  className={styles.inputField}
                  type="text"
                  placeholder="Indtast email eller brugernavn"
                  value={loginUsername}
                  onChange={(e) => setLoginUsername(e.target.value)}
                  required
                />
              </div>
  
              <div className={styles.formGroup}>
                <p>Adgangskode</p>
                <input
                  className={styles.inputField}
                  type="password"
                  placeholder="Indtast adgangskode"
                  value={loginPassword}
                  onChange={(e) => setLoginPassword(e.target.value)}
                  required
                />

              </div>
  
              <button className={styles.button} type="submit">Log på</button>
            </form>
  
            <p className={styles.linkText} onClick={() => setShowForgotPassword(true)}>Glemt adgangskode?</p>
            <p>Log på med</p>
            <button
              type="button"
              onClick={handleGoogleLogin} 
              className={styles.googleLoginButton}
              aria-label="Log på med Google" 
            >
            </button>
            <p className={styles.noAccount}>
              Har du ikke en konto? Opret en <span className={styles.linkText} onClick={() => setStartSlide(true)}>her</span>
            </p>
          </div>
        </div>
  
        {/* Register Form Section */}
        <div className={styles.registerForm}>
          <div className={styles.formContent}>
            <h1>Opret en konto</h1>
            {/* {success && <p style={{ color: "green" }}>{success}</p>} */}
            <form onSubmit={handleRegister} className={styles.formContainer}>
            <div className={styles.formGroup}>
            <p>Brugernavn</p>
              <input
                className={styles.inputField}
                type="text"
                placeholder="Indtast brugernavn"
                value={registerUsername}
                onChange={(e) => setRegisterUsername(e.target.value)}
                required
              />
              </div>
              <div className={styles.formGroup}>
              <p>Email</p>
              <input
                className={styles.inputField}
                type="email"
                placeholder="Indtast email"
                value={registerEmail}
                onChange={(e) => setRegisterEmail(e.target.value)}
                required
              />
              </div>
              <div className={styles.formGroup}>
              <p>Adgangskode</p>
              <input
                className={styles.inputField}
                type="password"
                placeholder="Indtast adgangskode"
                value={registerPassword}
                onChange={(e) => setRegisterPassword(e.target.value)}
                required
              />
              </div>
              <button className={styles.button} type="submit">Opret konto</button>
            </form>
  
            <p className={styles.linkText} onClick={() => setStartSlide(false)}>
              Tilbage til login
            </p>
          </div>
        </div>
  
        {/* Image Section */}
        <div className={styles.imageSection}>
          <img src={loginImage} className={styles.loginImage} alt="Login visual" />
        </div>
      </div>

    {showPopup && statusMessage && statusHeader && (
      <div className={styles.popupOverlay} onClick={() => setShowPopup(false)}>
        <div className={styles.popupContent} onClick={(e) => e.stopPropagation()}>
        <button className={styles.closeButton} onClick={() => setShowPopup(false)}>
          &times;
        </button>
        <h2>{statusHeader}</h2>
        <p>{statusMessage}</p>
    </div>
    </div>
    )}

    {showForgotPassword && (
      <div className={styles.popupOverlay} onClick={() => setShowForgotPassword(false)}>
        <div className={styles.popupContent} onClick={(e) => e.stopPropagation()}>
          <button className={styles.closeButton} onClick={() => setShowForgotPassword(false)}>
            &times;
          </button>
          <h2>Nulstil adgangskode</h2>
          <form onSubmit={handleForgotPassword} className={styles.formContainer}>
            <div className={styles.formGroup}>
              <p>Email</p>
              <input
                className={styles.inputField}
                type="email"
                placeholder="Indtast email"
                value={forgotPasswordEmail}
                onChange={(e) => setForgotPasswordEmail(e.target.value)}
                required
              />
            </div>
            <button className={`${styles.button} ${styles.forgotPasswordButton}`} type="submit">Send nulstillingslink</button>
          </form>
        </div>
      </div>
    )};
  </div>
  );
};

export default Login;