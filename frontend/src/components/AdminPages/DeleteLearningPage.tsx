import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import "./ChangeLearningPage.css";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import { fetchPagesStructure } from "../../services/ApiService";
import type { PageSummaryDto, PageDetailDto as ApiPageDetailDto, QuestionDto as ApiQuestionDto } from "../../types/pageTypes";

export default function DeleteLearningPage() {
  const [pages, setPages] = useState<PageSummaryDto[]>([]);
  const [selectedPageId, setSelectedPageId] = useState<number | null>(null);
  const [pageDetails, setPageDetails] = useState<ApiPageDetailDto | null>(null);
  const navigate = useNavigate();

  const token = localStorage.getItem("jwt");

  useEffect(() => {
    const loadPages = async () => {
      try {
        const data = await fetchPagesStructure();
        setPages(data);
      } catch (err) {
        console.error("Fejl ved hentning af sider:", err);
        // Optionally, set an error state here to display to the user
      }
    };
    loadPages();
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
      .then((res) => setPageDetails(res.data as ApiPageDetailDto))
      .catch(console.error);
  }, [selectedPageId, token]);

  const handleDelete = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedPageId) return;

    if (!confirm("Er du sikker på, at du vil slette denne læringsside?")) return;

    try {
      await axios.delete(`/api/pages/${selectedPageId}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      alert("Siden er slettet.");
      navigate("/admin/Laering");
    } catch (error) {
      console.error("Failed to delete page", error);
      alert("Sletning fejlede.");
    }
  };

  return (
    <div className="container">
      <div>
        <img src={BorgertingetIcon} className="Borgertinget-Icon" alt="Borgertinget Icon" />
      </div>
      <div className="top-red-line"></div>
      <h1>Slet Læringsside</h1>
      <p className="subtitle">Vælg en læringsside for at se og slette den</p>

      <div className="page-section">
        <label htmlFor="select-page" className="page-label">
          Vælg Side
        </label>
        <select id="select-page" className="page-input" value={selectedPageId ?? ""} onChange={(e) => setSelectedPageId(Number(e.target.value))}>
          <option value="">-- Vælg en side --</option>
          {pages.map((page) => (
            <option key={page.id} value={page.id}>
              {page.title}
            </option>
          ))}
        </select>
      </div>

      {pageDetails && (
        <form onSubmit={handleDelete} className="page-form">
          <div className="page-section">
            <label htmlFor="page-title" className="page-label">
              Titel
            </label>
            <input id="page-title" type="text" value={pageDetails.title} disabled className="page-input" />
          </div>

          <div className="page-section">
            <label htmlFor="page-content" className="page-label">
              Indhold
            </label>
            <textarea id="page-content" value={pageDetails.content} disabled rows={6} className="page-input page-textarea" />
          </div>

          {pageDetails.associatedQuestions && pageDetails.associatedQuestions.length > 0 && (
            <div className="questions-admin-section">
              <h2>Tilknyttede Spørgsmål (vil også blive slettet)</h2>
              {pageDetails.associatedQuestions.map((question: ApiQuestionDto) => (
                <div key={question.id} className="question-admin-block read-only">
                  <p className="page-label">
                    <strong>Spørgsmål:</strong> {question.questionText}
                  </p>
                  {question.options && question.options.length > 0 && (
                    <div>
                      <h4>Svarsmuligheder:</h4>
                      <ul>
                        {question.options.map((option) => (
                          <li key={option.id}>
                            {option.optionText}
                            {(option as any).isCorrect && <span className="correct-indicator"> (Korrekt)</span>}
                          </li>
                        ))}
                      </ul>
                    </div>
                  )}
                  {(!question.options || question.options.length === 0) && <p className="no-options-text">Ingen svarsmuligheder tilknyttet.</p>}
                </div>
              ))}
            </div>
          )}

          <div className="page-buttons">
            <button type="submit" className="page-btn delete-action-btn">
              Slet Side
            </button>
          </div>
        </form>
      )}
    </div>
  );
}
