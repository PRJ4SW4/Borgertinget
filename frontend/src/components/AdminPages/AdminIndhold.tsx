import { useNavigate } from "react-router-dom";
import axios from "axios";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import "./AdminIndhold.css";
import { useState } from "react";

export default function AdminIndhold() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [updateSuccess, setUpdateSuccess] = useState(false);

  const handleFetchActors = async () => {
    const token = localStorage.getItem("jwt");
    if (!token) {
      alert("Du er ikke logget ind.");
      return;
    }

    setLoading(true);
    setMessage("");
    setUpdateSuccess(false);

    try {
      await axios.post("/api/aktor/fetch", null, {
        headers: { Authorization: `Bearer ${token}` },
      });

      setMessage("Aktører blev opdateret!");
      setUpdateSuccess(true);
    } catch (error) {
      console.error("Fejl ved opdatering af aktører", error);
      setMessage("Fejl: Kunne ikke opdatere aktører.");
      setUpdateSuccess(false);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container">
      <div>
        <img src={BorgertingetIcon} className="Borgertinget-Icon" alt="Borgertinget Icon" />
      </div>
      <div className="top-red-line"></div>

      <h1>Administrer Indhold</h1>
      <div className="button-group">
        <button onClick={handleFetchActors} className={`Button ${updateSuccess ? "Button-success" : ""}`} disabled={loading}>
          {loading ? "Opdaterer..." : "Hent alle partier og politikere"}
        </button>

        {message && <p style={{ marginTop: "1rem", fontWeight: "bold", color: updateSuccess ? "green" : "red" }}>{message}</p>}

        <br />
        <button onClick={() => navigate("/admin/Indhold/redigerIndhold")} className="Button">
          Rediger Indhold
        </button>
        <br />
        <button onClick={() => navigate("/admin/Indhold/tilføjBegivenhed")} className="Button">
          Tilføj Begivenhed
        </button>
        <br />
        <button onClick={() => navigate("/admin/Indhold/redigerBegivenhed")} className="Button">
          Rediger Begivenhed
        </button>
        <br />
        <button onClick={() => navigate("/admin/Indhold/sletBegivenhed")} className="Button">
          Slet Begivenhed
        </button>
      </div>
    </div>
  );
}
