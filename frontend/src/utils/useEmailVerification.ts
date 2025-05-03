import { useEffect, useRef} from "react";
import axios from "axios";

const useEmailVerification = (
    onSuccess: (message: string) => void,
    onError: (message: string) => void
) => { 
    const hasRun = useRef(false);
    useEffect(() => {
        // Check if the effect has already run
        if (hasRun.current) return;
        hasRun.current = true;

      const params = new URLSearchParams(window.location.search);
      const token = params.get("token");
      
      if (token) {
        axios.get(`http://localhost:5218/api/users/verify?token=${token}`)
          .then((res) => {
            if (res.status === 200 && res.data?.message) {
              onSuccess(res.data.message || "Email verificeret.")
            } else {
                onError("Verifikation fejlede nummer 1.");
                }
            })
            .catch((err) => {
                console.error("Verification error:", err);
                if (err.response?.status === 400) {
                  onError(err.response.data || "Verifikation fejlede.");
                } else {
                  onError("Ingen forbindelse til serveren.");
                }
              })
              .finally(() => {
                window.history.replaceState({}, document.title, window.location.pathname);
              });
      }
    }, []);
  };
  
  export default useEmailVerification;