import { useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import "./ChangeEvent.css"; // Use the new CSS file
import BorgertingetIcon from "../../images/BorgertingetIcon.png";

interface CalendarEventData {
  title: string;
  startDateTimeUtc: string;
  location?: string;
  sourceUrl: string;
}

export default function AddEvent() {
  const navigate = useNavigate();
  const [eventData, setEventData] = useState<CalendarEventData>({
    title: "",
    startDateTimeUtc: "",
    location: "",
    sourceUrl: "",
  });
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [isError, setIsError] = useState(false);

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

    // Ensure datetime is in ISO 8601 format if using datetime-local input
    // The HTML datetime-local input typically provides a format like "YYYY-MM-DDTHH:mm"
    // We need to ensure it's compatible with what the backend expects for DateTimeOffset.
    // Appending ':00Z' or ensuring the correct offset is included might be necessary
    // depending on backend parsing. For simplicity, we assume the direct value works
    // or needs to be adjusted to a full ISO 8601 string with timezone.
    // For UTC, "Z" should be appended.
    let isoDateTime = eventData.startDateTimeUtc;
    if (isoDateTime && !isoDateTime.endsWith("Z") && isoDateTime.includes("T")) {
      isoDateTime += ":00Z"; // Assuming seconds are 00 and it's UTC
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
      setEventData({ title: "", startDateTimeUtc: "", location: "", sourceUrl: "" }); // Reset form
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
      <div>
        <img src={BorgertingetIcon} className="Borgertinget-Icon" alt="Borgertinget Icon" />
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
            type="datetime-local" // Provides a date and time picker
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
