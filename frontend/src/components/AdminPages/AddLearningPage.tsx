import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { fetchPagesStructure } from "../../services/ApiService";
import type { PageSummaryDto } from "../../types/pageTypes";
import "./ChangeLearningPage.css"; // Updated CSS import

export default function AddLearningPage() {
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [parentId, setParentId] = useState<number | null>(null);
  const [pages, setPages] = useState<PageSummaryDto[]>([]);
  const navigate = useNavigate();

  useEffect(() => {
    const loadPages = async () => {
      try {
        const data = await fetchPagesStructure();
        setPages(data);
      } catch (err) {
        console.error("Fejl ved hentning af sider:", err);
      }
    };
    loadPages();
  }, []);

  const handleSubmit = async () => {
    if (!title.trim()) {
      alert("Titel må ikke være tom.");
      return;
    }

    const payload = {
      title,
      content,
      parentPageId: parentId,
      displayOrder: 1,
    };

    try {
      const response = await fetch("/api/pages", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("jwt")}`,
        },
        body: JSON.stringify(payload),
      });

      if (response.ok) {
        alert("Læringsside oprettet!");
        navigate("/admin");
      } else {
        const errorText = await response.text();
        console.error("Serverfejl:", errorText);
        alert("Fejl under oprettelse af side.");
      }
    } catch (err) {
      console.error("Netværksfejl:", err);
      alert("Netværksfejl ved oprettelse af side.");
    }
  };

  return (
    <div className="container">
      <h1>Opret Læringsside</h1>

      <input type="text" placeholder="Titel" value={title} onChange={(e) => setTitle(e.target.value)} className="add-poll-title" />

      <textarea
        className="add-poll-form"
        placeholder="Indhold (Markdown understøttet)"
        value={content}
        onChange={(e) => setContent(e.target.value)}
        rows={6}
      />

      <select
        value={parentId ?? ""}
        onChange={(e) => {
          const value = e.target.value;
          setParentId(value === "" ? null : Number(value));
        }}
        className="add-poll-input">
        <option value="">(Ingen overordnet side)</option>
        {pages.map((page) => (
          <option key={page.id} value={page.id}>
            {page.title}
          </option>
        ))}
      </select>

      <button onClick={handleSubmit} className="add-poll-submit-btn">
        Gem Side
      </button>
    </div>
  );
}
