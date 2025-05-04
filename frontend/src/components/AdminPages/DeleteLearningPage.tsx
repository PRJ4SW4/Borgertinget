import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import "./ChangeLearningPage.css";

interface PageSummaryDto {
  id: number;
  title: string;
  parentPageId: number | null;
  displayOrder: number;
  hasChildren: boolean;
}

interface PageDetailDto {
  id: number;
  title: string;
  content: string;
  parentPageId: number | null;
}

export default function DeleteLearningPage() {
  const [pages, setPages] = useState<PageSummaryDto[]>([]);
  const [selectedPageId, setSelectedPageId] = useState<number | null>(null);
  const [pageDetails, setPageDetails] = useState<PageDetailDto | null>(null);
  const navigate = useNavigate();

  const token = localStorage.getItem("jwt");

  useEffect(() => {
    axios
      .get("/api/pages", {
        headers: { Authorization: `Bearer ${token}` },
      })
      .then((res) => setPages(res.data))
      .catch(console.error);
  }, []);

  useEffect(() => {
    if (!selectedPageId) {
      setPageDetails(null);
      return;
    }

    axios
      .get(`/api/pages/${selectedPageId}`, {
        headers: { Authorization: `Bearer ${token}` },
      })
      .then((res) => setPageDetails(res.data))
      .catch(console.error);
  }, [selectedPageId]);

  const handleDelete = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedPageId) return;

    if (!confirm("Er du sikker på, at du vil slette denne læringsside?")) return;

    try {
      await axios.delete(`/api/pages/${selectedPageId}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      alert("Siden er slettet.");
      navigate("/admin");
    } catch (error) {
      console.error("Failed to delete page", error);
      alert("Sletning fejlede.");
    }
  };

  return (
    <div className="add-poll-container">
      <h1 className="add-poll-title">Slet Læringsside</h1>
      <p className="add-poll-subtitle">Vælg en læringsside for at se og slette den</p>

      <div className="add-poll-section">
        <label className="add-poll-label">Vælg Side</label>
        <select className="add-poll-input" value={selectedPageId ?? ""} onChange={(e) => setSelectedPageId(Number(e.target.value))}>
          <option value="">-- Vælg en side --</option>
          {pages.map((page) => (
            <option key={page.id} value={page.id}>
              {page.title}
            </option>
          ))}
        </select>
      </div>

      {pageDetails && (
        <form onSubmit={handleDelete} className="add-poll-form">
          <div className="add-poll-section">
            <label className="add-poll-label">Titel</label>
            <input type="text" value={pageDetails.title} disabled className="add-poll-input" />
          </div>

          <div className="add-poll-section">
            <label className="add-poll-label">Indhold</label>
            <textarea value={pageDetails.content} disabled rows={6} className="add-poll-input" />
          </div>

          <div className="add-poll-buttons">
            <button type="submit" className="add-poll-submit-btn" style={{ backgroundColor: "#991b1b" }}>
              Slet Side
            </button>
          </div>
        </form>
      )}
    </div>
  );
}
