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
      // Optionally navigate away or show success for longer
      // navigate("/admin/Indhold");
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
      <h1 className="add-poll-title">Tilføj Ny Begivenhed</h1>
      <p className="add-poll-subtitle">Udfyld detaljerne for den nye kalenderbegivenhed.</p>

      <form onSubmit={handleSubmit} className="add-poll-form">
        <div className="add-poll-section">
          <label className="add-poll-label" htmlFor="title">
            Titel <span style={{ color: "red" }}>*</span>
          </label>
          <input
            id="title"
            name="title"
            type="text"
            value={eventData.title}
            onChange={handleChange}
            className="add-poll-input"
            placeholder="Begivenhedens titel"
            required
          />
        </div>

        <div className="add-poll-section">
          <label className="add-poll-label" htmlFor="startDateTimeUtc">
            Start Dato/Tid (UTC) <span style={{ color: "red" }}>*</span>
          </label>
          <input
            id="startDateTimeUtc"
            name="startDateTimeUtc"
            type="datetime-local" // Provides a date and time picker
            value={eventData.startDateTimeUtc}
            onChange={handleChange}
            className="add-poll-input"
            required
          />
          <small>Vælg dato og tid. Tiden vil blive fortolket som UTC.</small>
        </div>

        <div className="add-poll-section">
          <label className="add-poll-label" htmlFor="location">
            Lokation
          </label>
          <input
            id="location"
            name="location"
            type="text"
            value={eventData.location}
            onChange={handleChange}
            className="add-poll-input"
            placeholder="Begivenhedens lokation (valgfri)"
          />
        </div>

        <div className="add-poll-section">
          <label className="add-poll-label" htmlFor="sourceUrl">
            Kilde URL <span style={{ color: "red" }}>*</span>
          </label>
          <input
            id="sourceUrl"
            name="sourceUrl"
            type="text"
            value={eventData.sourceUrl}
            onChange={handleChange}
            className="add-poll-input"
            placeholder="URL til kilden (f.eks. Altinget)"
            required
          />
        </div>

        {message && <p style={{ color: isError ? "red" : "green", fontWeight: "bold" }}>{message}</p>}

        <div className="add-poll-buttons">
          <button type="submit" className="add-poll-submit-btn" disabled={loading}>
            {loading ? "Opretter..." : "Opret Begivenhed"}
          </button>
          <button type="button" className="add-poll-remove-btn" onClick={() => navigate("/admin/Indhold")} style={{ backgroundColor: "#6c757d" }}>
            Tilbage til Admin Indhold
          </button>
        </div>
      </form>
    </div>
  );
}
