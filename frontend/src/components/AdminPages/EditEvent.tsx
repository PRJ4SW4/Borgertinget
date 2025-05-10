import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import "./ChangeEvent.css"; // Use the new CSS file
import BorgertingetIcon from "../../images/BorgertingetIcon.png";

interface CalendarEventDTO {
  id: number;
  title: string;
  startDateTimeUtc: string;
  location?: string;
  sourceUrl: string;
}

export default function EditEvent() {
  const navigate = useNavigate();
  const [events, setEvents] = useState<CalendarEventDTO[]>([]);
  const [selectedEventId, setSelectedEventId] = useState<number | null>(null);
  const [currentEvent, setCurrentEvent] = useState<CalendarEventDTO | null>(null);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [isError, setIsError] = useState(false);

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

  // Fetch details of the selected event
  useEffect(() => {
    if (selectedEventId === null) {
      setCurrentEvent(null);
      return;
    }
    const eventDetails = events.find((event) => event.id === selectedEventId);
    if (eventDetails) {
      // Format DateTime for datetime-local input
      // It expects "YYYY-MM-DDTHH:mm"
      let localDateTime = eventDetails.startDateTimeUtc;

      // Attempt to parse the date and reformat it
      try {
        const dateObj = new Date(localDateTime);
        // Check if the date is valid before formatting
        if (!isNaN(dateObj.getTime())) {
          const year = dateObj.getFullYear();
          const month = (dateObj.getMonth() + 1).toString().padStart(2, "0"); // Months are 0-indexed
          const day = dateObj.getDate().toString().padStart(2, "0");
          const hours = dateObj.getHours().toString().padStart(2, "0");
          const minutes = dateObj.getMinutes().toString().padStart(2, "0");
          localDateTime = `${year}-${month}-${day}T${hours}:${minutes}`;
        } else {
          // If parsing fails, log an error and try to use a simpler slice if it was a UTC string
          console.error("Invalid date string received:", eventDetails.startDateTimeUtc);
          if (eventDetails.startDateTimeUtc.endsWith("Z")) {
            localDateTime = eventDetails.startDateTimeUtc.slice(0, 16); // Attempt to get YYYY-MM-DDTHH:mm
          } else {
            localDateTime = ""; // Or set to a default/empty if it's completely unparseable
          }
        }
      } catch (e) {
        console.error("Error parsing date string:", eventDetails.startDateTimeUtc, e);
        localDateTime = ""; // Fallback to empty or a sensible default
      }

      setCurrentEvent({ ...eventDetails, startDateTimeUtc: localDateTime });
    } else {
      setCurrentEvent(null);
    }
  }, [selectedEventId, events]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    if (!currentEvent) return;
    const { name, value } = e.target;
    setCurrentEvent((prev) => ({
      ...prev!,
      [name]: value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!currentEvent || selectedEventId === null) {
      setMessage("Ingen begivenhed valgt eller data mangler.");
      setIsError(true);
      return;
    }
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

    if (!currentEvent.title || !currentEvent.startDateTimeUtc || !currentEvent.sourceUrl) {
      setMessage("Titel, Start Dato/Tid (UTC), og Kilde URL er påkrævede felter.");
      setIsError(true);
      setLoading(false);
      return;
    }

    let isoDateTime = currentEvent.startDateTimeUtc;
    if (isoDateTime && !isoDateTime.endsWith("Z") && isoDateTime.includes("T")) {
      isoDateTime += ":00Z"; // Add seconds and Z for UTC
    }

    try {
      await axios.put(
        `/api/calendar/events/${selectedEventId}`,
        { ...currentEvent, startDateTimeUtc: isoDateTime },
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );
      setMessage("Begivenhed opdateret succesfuldt!");
      setIsError(false);
      // Optionally, refresh the events list or navigate
      // navigate("/admin/Indhold");
    } catch (error) {
      console.error("Fejl ved opdatering af begivenhed:", error);
      if (axios.isAxiosError(error) && error.response) {
        setMessage(`Fejl: ${error.response.data.message || "Kunne ikke opdatere begivenhed."}`);
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
      <div>
        <img src={BorgertingetIcon} className="Borgertinget-Icon" alt="Borgertinget Icon" />
      </div>
      <div className="top-red-line"></div>
      <h1 className="add-poll-title">Rediger Begivenhed</h1>
      <p className="add-poll-subtitle">Vælg en begivenhed for at redigere dens detaljer.</p>

      <div className="add-poll-section">
        <label className="add-poll-label" htmlFor="eventSelect">
          Vælg Begivenhed
        </label>
        <select
          id="eventSelect"
          className="add-poll-input"
          value={selectedEventId ?? ""}
          onChange={(e) => setSelectedEventId(Number(e.target.value) || null)}
          disabled={loading}>
          <option value="">-- Vælg en begivenhed --</option>
          {events.map((event) => (
            <option key={event.id} value={event.id}>
              {event.title} (ID: {event.id})
            </option>
          ))}
        </select>
      </div>

      {currentEvent && selectedEventId !== null && (
        <form onSubmit={handleSubmit} className="add-poll-form">
          <div className="add-poll-section">
            <label className="add-poll-label" htmlFor="title">
              Titel <span style={{ color: "red" }}>*</span>
            </label>
            <input id="title" name="title" type="text" value={currentEvent.title} onChange={handleChange} className="add-poll-input" required />
          </div>

          <div className="add-poll-section">
            <label className="add-poll-label" htmlFor="startDateTimeUtc">
              Start Dato/Tid (UTC) <span style={{ color: "red" }}>*</span>
            </label>
            <input
              id="startDateTimeUtc"
              name="startDateTimeUtc"
              type="datetime-local"
              value={currentEvent.startDateTimeUtc}
              onChange={handleChange}
              className="add-poll-input"
              required
            />
            <small>Tiden vil blive fortolket som UTC ved gemning.</small>
          </div>

          <div className="add-poll-section">
            <label className="add-poll-label" htmlFor="location">
              Lokation
            </label>
            <input id="location" name="location" type="text" value={currentEvent.location ?? ""} onChange={handleChange} className="add-poll-input" />
          </div>

          <div className="add-poll-section">
            <label className="add-poll-label" htmlFor="sourceUrl">
              Kilde URL <span style={{ color: "red" }}>*</span>
            </label>
            <input
              id="sourceUrl"
              name="sourceUrl"
              type="text"
              value={currentEvent.sourceUrl}
              onChange={handleChange}
              className="add-poll-input"
              required
            />
          </div>

          {message && <p style={{ color: isError ? "red" : "green", fontWeight: "bold" }}>{message}</p>}

          <div className="add-poll-buttons">
            <button type="submit" className="add-poll-submit-btn" disabled={loading || !selectedEventId}>
              {loading ? "Opdaterer..." : "Opdater Begivenhed"}
            </button>
            <button type="button" className="add-poll-remove-btn" onClick={() => navigate("/admin/Indhold")} style={{ backgroundColor: "#6c757d" }}>
              Tilbage til Admin Indhold
            </button>
          </div>
        </form>
      )}
    </div>
  );
}
