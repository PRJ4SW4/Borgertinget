import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";

interface EmailVerificationProps {
  onVerified: () => void;
  onError: (message: string) => void;
}

const EmailVerification: React.FC<EmailVerificationProps> = ({ onVerified, onError }) => {
  const [verificationStatus, setVerificationStatus] = useState<'idle' | 'verifying' | 'success' | 'error'>('verifying');
  const navigate = useNavigate();

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const token = params.get("token");
    const userId = params.get("userId");

    if (token) {
      axios
        .get(`http://localhost:5218/api/users/verify?token=${token}&userId=${userId}`)
        .then((res) => {
          if (res.status === 200 && res.data?.message) {
            setVerificationStatus('success');
            onVerified(); // Notify parent component
            // Navigate to login with message
            navigate(`/login?message=${encodeURIComponent(res.data.message)}&status=success`);
          } else {
            setVerificationStatus('error');
            onError("Verifikation fejlede: Ugyldig token.");
          }
        })
        .catch((err) => {
          setVerificationStatus('error');
          console.error("Verification error:", err);
          if (err.response?.status === 400) {
            onError(err.response.data || "Verifikation fejlede.");
          } else {
            onError("Ingen forbindelse til serveren.");
          }
        })
        .finally(() => {
          // Remove token from URL
          window.history.replaceState({}, document.title, window.location.pathname);
        });
    } else {
      setVerificationStatus('error');
      onError("Verifikation fejlede: Token mangler i URL.");
    }
  }, [navigate, onVerified, onError]);

  if (verificationStatus === 'verifying') {
    return <div>Verificerer email...</div>; // Simple loading state
  }

  if (verificationStatus === 'error') {
    return null; //The error is already handled by the Login component and shown in a popup.
  }

  return null;
};

export default EmailVerification;