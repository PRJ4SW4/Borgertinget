import { useEffect } from "react";
import { useNavigate, useLocation} from "react-router-dom";

const ResetPasswordVerification: React.FC = () => {
    const navigate = useNavigate();
    const location = useLocation();
    useEffect(() => {
        const params = new URLSearchParams(location.search);
        const token = params.get("token");
        const userId = params.get("userId");

        if (token && userId) {
            navigate(`/login?userId=${userId}&token=${token}`);
            window.history.replaceState({}, document.title, window.location.pathname);
        } else {
      console.error("Ugyldigt link til nulstilling af adgangskode.");
      navigate('/login?message=Ugyldigt link til nulstilling af adgangskode.&status=error');
        }
    }, [navigate, location.search]);
    return (
        <div>
            <h1>Behandler nulstilling af adgangskode...</h1>
        </div>
  );
}
export default ResetPasswordVerification;