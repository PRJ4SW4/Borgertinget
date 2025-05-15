import { useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import axios from "axios";
import "./ChangeEvent.css";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import BackButton from "../Button/backbutton";

interface CalendarEventData {
  title: string;
  startDateTimeUtc: string;
  location?: string;
  sourceUrl: string;
}

export default function AddEvent() {
  const navigate = useNavigate();
  const location = useLocation(); // Added
  const [eventData, setEventData] = useState<CalendarEventData>({
    title: "",
    startDateTimeUtc: "",
    location: "",
    sourceUrl: "",
  });
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [isError, setIsError] = useState(false);

  const matchProp = { path: location.pathname }; // Added

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setEventData((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setMessage("");
    setIsError(false);

    const token = localStorage.getItem("jwt");
    if (!token) {
      setMessage("Du er ikke logget ind.");
      setIsError(true);
      setLoading(false);
      return;
    }

    if (!eventData.title || !eventData.startDateTimeUtc || !eventData.sourceUrl) {
      setMessage("Titel, Start Dato/Tid (UTC), og Kilde URL er påkrævede felter.");
      setIsError(true);
      setLoading(false);
      return;
    }

    let isoDateTime = eventData.startDateTimeUtc;
    if (isoDateTime && !isoDateTime.endsWith("Z") && isoDateTime.includes("T")) {
      isoDateTime += ":00Z";
    }

    try {
      await axios.post(
        "/api/calendar/events",
        { ...eventData, startDateTimeUtc: isoDateTime },
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );
      setMessage("Begivenhed oprettet succesfuldt!");
      setIsError(false);
      setEventData({ title: "", startDateTimeUtc: "", location: "", sourceUrl: "" });
      navigate("/admin/Indhold");
    } catch (error) {
      console.error("Fejl ved oprettelse af begivenhed:", error);
      if (axios.isAxiosError(error) && error.response) {
        setMessage(`Fejl: ${error.response.data.message || "Kunne ikke oprette begivenhed."}`);
      } else {
        setMessage("En ukendt fejl opstod.");
      }
      setIsError(true);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container">
      <div style={{ position: "relative" }}>
        {" "}
        {/* Added for positioning context */}
        <img src={BorgertingetIcon} className="Borgertinget-Icon" alt="Borgertinget Icon" />
        <div style={{ position: "absolute", top: "10px", left: "10px" }}>
          {" "}
          {/* Adjust top/left as needed */}
          <BackButton match={matchProp} destination="admin" />
        </div>
      </div>
      <div className="top-red-line"></div>
      <h1 className="event-title">Tilføj Ny Begivenhed</h1>
      <p className="event-subtitle">Udfyld detaljerne for den nye kalenderbegivenhed.</p>

      <form onSubmit={handleSubmit} className="event-form">
        <div className="event-section">
          <label className="event-label" htmlFor="title">
            Titel <span style={{ color: "red" }}>*</span>
          </label>
          <input
            id="title"
            name="title"
            type="text"
            value={eventData.title}
            onChange={handleChange}
            className="event-input"
            placeholder="Begivenhedens titel"
            required
          />
        </div>

        <div className="event-section">
          <label className="event-label" htmlFor="startDateTimeUtc">
            Start Dato/Tid (UTC) <span style={{ color: "red" }}>*</span>
          </label>
          <input
            id="startDateTimeUtc"
            name="startDateTimeUtc"
            type="datetime-local"
            value={eventData.startDateTimeUtc}
            onChange={handleChange}
            className="event-input"
            required
          />
          <small>Vælg dato og tid. Tiden vil blive fortolket som UTC.</small>
        </div>

        <div className="event-section">
          <label className="event-label" htmlFor="location">
            Lokation
          </label>
          <input
            id="location"
            name="location"
            type="text"
            value={eventData.location}
            onChange={handleChange}
            className="event-input"
            placeholder="Begivenhedens lokation (valgfri)"
          />
        </div>

        <div className="event-section">
          <label className="event-label" htmlFor="sourceUrl">
            Kilde URL <span style={{ color: "red" }}>*</span>
          </label>
          <input
            id="sourceUrl"
            name="sourceUrl"
            type="text"
            value={eventData.sourceUrl}
            onChange={handleChange}
            className="event-input"
            placeholder="URL til kilden (f.eks. Altinget)"
            required
          />
        </div>

        {message && <p style={{ color: isError ? "red" : "green", fontWeight: "bold" }}>{message}</p>}

        <div className="event-buttons">
          <button type="submit" className="event-submit-btn" disabled={loading}>
            {loading ? "Opretter..." : "Opret Begivenhed"}
          </button>
          <button type="button" className="event-remove-btn" onClick={() => navigate("/admin/Indhold")} style={{ backgroundColor: "#6c757d" }}>
            Tilbage til Admin Indhold
          </button>
        </div>
      </form>
    </div>
  );
}
