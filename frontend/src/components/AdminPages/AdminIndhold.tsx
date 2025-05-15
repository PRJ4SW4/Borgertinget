import { useNavigate, useLocation } from "react-router-dom";
import axios from "axios";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import "./AdminIndhold.css";
import { useState } from "react";
import BackButton from "../Button/backbutton";

export default function AdminIndhold() {
  const navigate = useNavigate();
  const location = useLocation();
  // State for fetching actors
  const [actorsLoading, setActorsLoading] = useState(false);
  const [actorsMessage, setActorsMessage] = useState("");
  const [actorsUpdateSuccess, setActorsUpdateSuccess] = useState(false);

  // State for fetching events
  const [eventsLoading, setEventsLoading] = useState(false);
  const [eventsMessage, setEventsMessage] = useState("");
  const [eventsUpdateSuccess, setEventsUpdateSuccess] = useState(false);

  const matchProp = { path: location.pathname };
  const handleFetchActors = async () => {
    const token = localStorage.getItem("jwt");
    if (!token) {
      alert("Du er ikke logget ind.");
      return;
    }

    setActorsLoading(true);
    setActorsMessage("");
    setActorsUpdateSuccess(false);
    // Clear event message when starting actor fetch
    setEventsMessage("");
    setEventsUpdateSuccess(false);

    try {
      await axios.post("/api/aktor/fetch", null, {
        headers: { Authorization: `Bearer ${token}` },
      });

      setActorsMessage("Aktører blev opdateret!");
      setActorsUpdateSuccess(true);
    } catch (error) {
      console.error("Fejl ved opdatering af aktører", error);
      setActorsMessage("Fejl: Kunne ikke opdatere aktører.");
      setActorsUpdateSuccess(false);
    } finally {
      setActorsLoading(false);
    }
  };

  const handleFetchEvents = async () => {
    const token = localStorage.getItem("jwt");
    if (!token) {
      alert("Du er ikke logget ind.");
      return;
    }

    setEventsLoading(true);
    setEventsMessage("");
    setEventsUpdateSuccess(false);
    // Clear actor message when starting event fetch
    setActorsMessage("");
    setActorsUpdateSuccess(false);

    try {
      await axios.post("/api/calendar/run-calendar-scraper", null, {
        headers: { Authorization: `Bearer ${token}` },
      });

      setEventsMessage("Begivenheder blev skrabet og automation kørt!");
      setEventsUpdateSuccess(true);
    } catch (error) {
      console.error("Fejl ved opdatering af begivenheder eller kørsel af automation", error);
      setEventsMessage("Fejl: Kunne ikke opdatere begivenheder eller køre automation.");
      setEventsUpdateSuccess(false);
    } finally {
      setEventsLoading(false);
    }
  };

  return (
    <div className="container">
      <div style={{ position: "relative" }}>
        {" "}
        <img src={BorgertingetIcon} className="Borgertinget-Icon" alt="Borgertinget Icon" />
        <div style={{ position: "absolute", top: "10px", left: "10px" }}>
          {" "}
          <BackButton match={matchProp} destination="admin" />
        </div>
      </div>
      <div className="top-red-line"></div>

      <h1>Administrer Indhold</h1>
      <div className="button-group">
        <button onClick={handleFetchActors} className={`Button ${actorsUpdateSuccess ? "Button-success" : ""}`} disabled={actorsLoading}>
          {actorsLoading ? "Opdaterer..." : "Hent alle partier og politikere"}
        </button>

        {actorsMessage && <p style={{ marginTop: "1rem", fontWeight: "bold", color: actorsUpdateSuccess ? "green" : "red" }}>{actorsMessage}</p>}

        <br />
        <button onClick={() => navigate("/admin/Indhold/redigerIndhold")} className="Button">
          Rediger Indhold
        </button>
        <br />
        <button onClick={handleFetchEvents} className={`Button ${eventsUpdateSuccess ? "Button-success" : ""}`} disabled={eventsLoading}>
          {eventsLoading ? "Opdaterer..." : "Hent alle begivenheder fra Altinget"}
        </button>

        {eventsMessage && <p style={{ marginTop: "1rem", fontWeight: "bold", color: eventsUpdateSuccess ? "green" : "red" }}>{eventsMessage}</p>}
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
