import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import "./ChangeLearningPage.css";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";

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

export default function EditLearningPage() {
  const [pages, setPages] = useState<PageSummaryDto[]>([]);
  const [selectedPageId, setSelectedPageId] = useState<number | null>(null);
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [parentPageId, setParentPageId] = useState<number | null>(null);
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
    if (!selectedPageId) return;
    axios
      .get(`/api/pages/${selectedPageId}`, {
        headers: { Authorization: `Bearer ${token}` },
      })
      .then((res) => {
        const page: PageDetailDto = res.data;
        setTitle(page.title);
        setContent(page.content);
        setParentPageId(page.parentPageId);
      })
      .catch(console.error);
  }, [selectedPageId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedPageId) return;

    try {
      await axios.put(
        `/api/pages/${selectedPageId}`,
        {
          id: selectedPageId,
          title,
          content,
          parentPageId,
          displayOrder: 1, // Can be made editable if needed
        },
        {
          headers: { Authorization: `Bearer ${token}` },
        }
      );
      alert("Siden er opdateret!");
      navigate("/admin/Laering");
    } catch (error) {
      console.error("Failed to update page", error);
      alert("Opdatering fejlede.");
    }
  };

  return (
    <div className="container">
      <div>
        <img src={BorgertingetIcon} className="Borgertinget-Icon" alt="Borgertinget Icon" />
      </div>
      <div className="top-red-line"></div>
      <h1>Rediger Læringsside</h1>

      <label className="page-label">Vælg Side</label>
      <select value={selectedPageId ?? ""} onChange={(e) => setSelectedPageId(Number(e.target.value))} className="page-input">
        <option value="">-- Vælg side --</option>
        {pages.map((p) => (
          <option key={p.id} value={p.id}>
            {p.title}
          </option>
        ))}
      </select>

      {selectedPageId && (
        <form onSubmit={handleSubmit}>
          <label className="page-label">Titel</label>
          <input type="text" value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Titel" className="page-title" />

          <label className="page-label">Indhold</label>
          <textarea value={content} onChange={(e) => setContent(e.target.value)} rows={6} placeholder="Markdown indhold" className="page-form" />

          <label className="page-label">Overordnet side</label>
          <select
            value={parentPageId ?? ""}
            onChange={(e) => setParentPageId(e.target.value === "" ? null : Number(e.target.value))}
            className="page-input">
            <option value="">(Ingen overordnet side)</option>
            {pages
              .filter((p) => p.id !== selectedPageId)
              .map((p) => (
                <option key={p.id} value={p.id}>
                  {p.title}
                </option>
              ))}
          </select>

          <button type="submit" className="page-btn">
            Gem Ændringer
          </button>
        </form>
      )}
    </div>
  );
}
