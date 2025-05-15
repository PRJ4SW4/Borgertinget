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

  // State for fetching search
  const [searchLoading, setSearchLoading] = useState(false);
  const [searchMessage, setSearchMessage] = useState("");
  const [searchUpdateSuccess, setSearchUpdateSuccess] = useState(false);

  // State for fetching all
  const [allLoading, setAllLoading] = useState(false);
  const [allMessage, setAllMessage] = useState("");
  const [allUpdateSuccess, setAllUpdateSuccess] = useState(false);

  const matchProp = { path: location.pathname };

  const handleFetchAll = async () => {
    const token = localStorage.getItem("jwt");
    if (!token) {
      alert("Du er ikke logget ind.");
      return;
    }

    setAllLoading(true);
    setAllMessage("");
    setAllUpdateSuccess(false);
    // Clear individual messages when starting fetch all
    setActorsMessage("");
    setActorsUpdateSuccess(false);
    setEventsMessage("");
    setEventsUpdateSuccess(false);
    setSearchMessage("");
    setSearchUpdateSuccess(false);

    try {
      await axios.post("/api/aktor/fetch", null, {
        headers: { Authorization: `Bearer ${token}` },
      });

      await axios.post("/api/Calendar/run-calendar-scraper", null, {
        headers: { Authorization: `Bearer ${token}` },
      });

      await axios.post("/api/polidle/admin/seed-all-aktor-quotes", null, {
        headers: { Authorization: `Bearer ${token}` },
      });

      await axios.post("/api/polidle/admin/generate-today", null, {
        headers: { Authorization: `Bearer ${token}` },
      });

      await axios.post("/api/Search/ensure-and-reindex", null, {
        headers: { Authorization: `Bearer ${token}` },
      });

      setAllMessage("Alle data blev opdateret!");
      setAllUpdateSuccess(true);
    } catch (error) {
      console.error("Fejl ved opdatering af alt indhold", error);
      setAllMessage("Fejl: Kunne ikke opdatere alt indhold.");
      setAllUpdateSuccess(false);
    } finally {
      setAllLoading(false);
    }
  };

  const handleFetchActors = async () => {
    const token = localStorage.getItem("jwt");
    if (!token) {
      alert("Du er ikke logget ind.");
      return;
    }

    setActorsLoading(true);
    setActorsMessage("");
    setActorsUpdateSuccess(false);
    // Clear other messages when starting actor fetch
    setEventsMessage("");
    setEventsUpdateSuccess(false);
    setSearchMessage("");
    setSearchUpdateSuccess(false);
    setAllMessage("");
    setAllUpdateSuccess(false);

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
    // Clear other messages when starting event fetch
    setActorsMessage("");
    setActorsUpdateSuccess(false);
    setSearchMessage("");
    setSearchUpdateSuccess(false);
    setAllMessage("");
    setAllUpdateSuccess(false);

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

  const handleFetchSearch = async () => {
    const token = localStorage.getItem("jwt");
    if (!token) {
      alert("Du er ikke logget ind.");
      return;
    }

    setSearchLoading(true);
    setSearchMessage("");
    setSearchUpdateSuccess(false);
    // Clear other messages when starting search fetch
    setActorsMessage("");
    setActorsUpdateSuccess(false);
    setEventsMessage("");
    setEventsUpdateSuccess(false);
    setAllMessage("");
    setAllUpdateSuccess(false);

    try {
      await axios.post("/api/Search/ensure-and-reindex", null, {
        headers: { Authorization: `Bearer ${token}` },
      });

      setSearchMessage("Search blev forsikret og indexing er færdig!");
      setSearchUpdateSuccess(true);
    } catch (error) {
      console.error("Fejl ved opdatering af search indexing", error);
      setSearchMessage("Fejl: Kunne ikke opdatere search index.");
      setSearchUpdateSuccess(false);
    } finally {
      setSearchLoading(false);
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
        <br />
        <button onClick={handleFetchAll} className={`Button ${allUpdateSuccess ? "Button-success" : ""}`} disabled={allLoading}>
          {allLoading ? "Henter alt..." : "Hent alt"}
        </button>

        {allMessage && <p style={{ marginTop: "1rem", fontWeight: "bold", color: allUpdateSuccess ? "green" : "red" }}>{allMessage}</p>}
        <br />
        <button onClick={handleFetchActors} className={`Button ${actorsUpdateSuccess ? "Button-success" : ""}`} disabled={actorsLoading}>
          {actorsLoading ? "Opdaterer..." : "Hent alle partier og politikere"}
        </button>

        {actorsMessage && <p style={{ marginTop: "1rem", fontWeight: "bold", color: actorsUpdateSuccess ? "green" : "red" }}>{actorsMessage}</p>}
        <br />
        <button onClick={handleFetchEvents} className={`Button ${eventsUpdateSuccess ? "Button-success" : ""}`} disabled={eventsLoading}>
          {eventsLoading ? "Opdaterer..." : "Hent alle begivenheder fra Altinget"}
        </button>

        {eventsMessage && <p style={{ marginTop: "1rem", fontWeight: "bold", color: eventsUpdateSuccess ? "green" : "red" }}>{eventsMessage}</p>}
        <br />
        <button onClick={handleFetchSearch} className={`Button ${searchUpdateSuccess ? "Button-success" : ""}`} disabled={searchLoading}>
          {searchLoading ? "Opdaterer..." : "Genkør Search indexeringen"}
        </button>

        {searchMessage && <p style={{ marginTop: "1rem", fontWeight: "bold", color: searchUpdateSuccess ? "green" : "red" }}>{searchMessage}</p>}
      </div>
    </div>
  );
}
