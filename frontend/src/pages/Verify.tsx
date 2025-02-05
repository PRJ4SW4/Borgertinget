import { useEffect, useState } from "react";
import axios from "axios";
import { useSearchParams } from "react-router-dom";

const VerifyEmail = () => {
  const [searchParams] = useSearchParams();
  const [message, setMessage] = useState("");

  useEffect(() => {
    const token = searchParams.get("token");
    if (token) {
      axios.get(`http://localhost:5218/api/users/verify?token=${token}`)
        .then(res => setMessage(res.data.message))
        .catch(err => {
          if(!message) {
            console.log("Fejlstatus:", err.response?.status);
            console.log("Fejlbesked:", err.response?.data);
            setMessage("Verifikation fejlede.");
          }
        });
    }
  }, []);

  return <div>{message}</div>;
};

export default VerifyEmail;
