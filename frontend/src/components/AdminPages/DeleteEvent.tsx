import { useEffect, useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import axios from "axios";
import "./ChangeEvent.css"; // Use the new CSS file
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import BackButton from "../Button/backbutton";

interface CalendarEventDTO {
  id: number;
  title: string;
  startDateTimeUtc: string;
  location?: string;
  sourceUrl: string;
}

export default function DeleteEvent() {
  const navigate = useNavigate();
  const location = useLocation();
  const [events, setEvents] = useState<CalendarEventDTO[]>([]);
  const [selectedEventId, setSelectedEventId] = useState<number | null>(null);
  const [currentEvent, setCurrentEvent] = useState<CalendarEventDTO | null>(null);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [isError, setIsError] = useState(false);

  const matchProp = { path: location.pathname }; // For BackButton

  // Fetch all events for the dropdown
  useEffect(() => {
    const fetchEvents = async () => {
      const token = localStorage.getItem("jwt");
      if (!token) {
        setMessage("Du er ikke logget ind.");
        setIsError(true);
        return;
      }
      try {
        setLoading(true);
        const response = await axios.get("/api/calendar/events", {
          headers: { Authorization: `Bearer ${token}` },
        });
        setEvents(response.data);
        setLoading(false);
      } catch (error) {
        console.error("Fejl ved hentning af begivenheder:", error);
        setMessage("Kunne ikke hente begivenheder.");
        setIsError(true);
        setLoading(false);
      }
    };
    fetchEvents();
  }, []);

  // Set details of the selected event for display
  useEffect(() => {
    if (selectedEventId === null) {
      setCurrentEvent(null);
      return;
    }
    const eventDetails = events.find((event) => event.id === selectedEventId);
    setCurrentEvent(eventDetails || null);
  }, [selectedEventId, events]);

  const handleDelete = async () => {
    if (selectedEventId === null) {
      setMessage("Ingen begivenhed valgt.");
      setIsError(true);
      return;
    }

    const confirmDelete = window.confirm("Er du sikker på, at du vil slette denne begivenhed?");
    if (!confirmDelete) return;

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

    try {
      await axios.delete(`/api/calendar/events/${selectedEventId}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      setMessage("Begivenhed slettet succesfuldt!");
      setIsError(false);
      setEvents(events.filter((event) => event.id !== selectedEventId)); // Remove from list
      setSelectedEventId(null); // Reset selection
      setCurrentEvent(null);
      // navigate("/admin/Indhold"); // Optionally navigate away
    } catch (error) {
      console.error("Fejl ved sletning af begivenhed:", error);
      if (axios.isAxiosError(error) && error.response) {
        setMessage(`Fejl: ${error.response.data.message || "Kunne ikke slette begivenhed."}`);
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
        <img src={BorgertingetIcon} className="Borgertinget-Icon" alt="Borgertinget Icon" />
        <div style={{ position: "absolute", top: "10px", left: "10px" }}>
          {" "}
          <BackButton match={matchProp} destination="admin" />
        </div>
      </div>
      <div className="top-red-line"></div>
      <h1 className="event-title">Slet Begivenhed</h1>
      <p className="event-subtitle">Vælg en begivenhed for at slette den.</p>

      <div className="event-section">
        <label className="event-label" htmlFor="eventSelectDelete">
          Vælg Begivenhed
        </label>
        <select
          id="eventSelectDelete"
          className="event-input"
          value={selectedEventId ?? ""}
          onChange={(e) => setSelectedEventId(Number(e.target.value) || null)}
          disabled={loading || events.length === 0}>
          <option value="">-- Vælg en begivenhed --</option>
          {events.map((event) => (
            <option key={event.id} value={event.id}>
              {event.title} (ID: {event.id})
            </option>
          ))}
        </select>
      </div>

      {currentEvent && selectedEventId !== null && (
        <div className="event-form" style={{ marginTop: "2rem" }}>
          <h3 style={{ color: "#333" }}>Begivenhedsdetaljer:</h3>
          <p>
            <strong>Titel:</strong> {currentEvent.title}
          </p>
          <p>
            <strong>Start:</strong> {new Date(currentEvent.startDateTimeUtc).toLocaleString()}
          </p>
          <p>
            <strong>Lokation:</strong> {currentEvent.location || "Ikke specificeret"}
          </p>
          <p>
            <strong>Kilde URL:</strong> {currentEvent.sourceUrl}
          </p>
        </div>
      )}

      {message && <p style={{ color: isError ? "red" : "green", fontWeight: "bold", marginTop: "1rem" }}>{message}</p>}

      <div className="event-buttons" style={{ marginTop: "2rem" }}>
        <button onClick={handleDelete} className="event-submit-btn" disabled={loading || !selectedEventId}>
          {loading ? "Sletter..." : "Slet Begivenhed"}
        </button>
        <button type="button" className="event-remove-btn" onClick={() => navigate("/admin/Indhold")} style={{ backgroundColor: "#6c757d" }}>
          Tilbage til Admin Indhold
        </button>
      </div>
    </div>
  );
}
