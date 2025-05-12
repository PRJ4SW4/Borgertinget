import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import type { PageSummaryDto, PageDetailDto as ApiPageDetailDto } from "../../types/pageTypes";
import { fetchPagesStructure } from "../../services/ApiService";
import "./ChangeLearningPage.css";

interface AnswerOptionFormState {
  id: number;
  optionText: string;
  isCorrect: boolean;
  displayOrder: number;
}

interface QuestionFormState {
  id: number;
  questionText: string;
  options: AnswerOptionFormState[];
}

export default function EditLearningPage() {
  const [pages, setPages] = useState<PageSummaryDto[]>([]);
  const [selectedPageId, setSelectedPageId] = useState<number | null>(null);
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [parentPageId, setParentPageId] = useState<number | null>(null);
  const [displayOrder, setDisplayOrder] = useState(0);
  const [questions, setQuestions] = useState<QuestionFormState[]>([]);
  const navigate = useNavigate();
  const token = localStorage.getItem("jwt");
  const [nextQuestionTempId, setNextQuestionTempId] = useState(-1);
  const [nextOptionTempId, setNextOptionTempId] = useState(-1);

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
      setTitle("");
      setContent("");
      setParentPageId(null);
      setDisplayOrder(0);
      setQuestions([]);
      return;
    }
    axios
      .get(`/api/pages/${selectedPageId}`, {
        headers: { Authorization: `Bearer ${token}` },
      })
      .then((res) => {
        const page: ApiPageDetailDto = res.data;
        setTitle(page.title);
        setContent(page.content);
        setParentPageId(page.parentPageId);
        const summary = pages.find((p) => p.id === selectedPageId);
        setDisplayOrder(summary?.displayOrder || 0);
        const formQuestions =
          page.associatedQuestions?.map((qApi) => ({
            id: qApi.id,
            questionText: qApi.questionText,
            options:
              qApi.options.map((optApi) => ({
                id: optApi.id,
                optionText: optApi.optionText,
                isCorrect: optApi.isCorrect,
                displayOrder: optApi.displayOrder,
              })) || [],
          })) || [];
        setQuestions(formQuestions);
      })
      .catch(console.error);
  }, [selectedPageId, token, pages]);

  const handleAddQuestion = () => {
    setQuestions([
      ...questions,
      {
        id: nextQuestionTempId,
        questionText: "",
        options: [
          { id: nextOptionTempId, optionText: "", isCorrect: false, displayOrder: 0 },
          { id: nextOptionTempId - 1, optionText: "", isCorrect: false, displayOrder: 1 },
        ],
      },
    ]);
    setNextQuestionTempId((prev) => prev - 1);
    setNextOptionTempId((prev) => prev - 2);
  };

  const handleQuestionTextChange = (qIndex: number, text: string) => {
    const newQuestions = questions.map((q, i) => (i === qIndex ? { ...q, questionText: text } : q));
    setQuestions(newQuestions);
  };

  const handleRemoveQuestion = (qIndex: number) => {
    setQuestions(questions.filter((_, index) => index !== qIndex));
  };

  const handleAddOption = (qIndex: number) => {
    const newQuestions = [...questions];
    newQuestions[qIndex].options.push({
      id: nextOptionTempId,
      optionText: "",
      isCorrect: false,
      displayOrder: newQuestions[qIndex].options.length,
    });
    setQuestions(newQuestions);
    setNextOptionTempId((prev) => prev - 1);
  };

  const handleOptionTextChange = (qIndex: number, oIndex: number, text: string) => {
    const newQuestions = [...questions];
    newQuestions[qIndex].options[oIndex].optionText = text;
    setQuestions(newQuestions);
  };

  const handleOptionCorrectChange = (qIndex: number, oIndex: number, checked: boolean) => {
    const newQuestions = [...questions];
    newQuestions[qIndex].options[oIndex].isCorrect = checked;
    setQuestions(newQuestions);
  };

  const handleRemoveLastOption = (qIndex: number) => {
    const newQuestions = [...questions];
    if (newQuestions[qIndex].options.length > 0) {
      newQuestions[qIndex].options.pop();
      setQuestions(newQuestions);
    } else {
      alert("Der er ingen svarmuligheder at fjerne.");
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedPageId) return;

    const questionsPayload = questions.map((q) => ({
      ...q,
      id: q.id < 0 ? 0 : q.id,
      options: q.options.map((opt) => ({
        ...opt,
        id: opt.id < 0 ? 0 : opt.id,
      })),
    }));

    try {
      await axios.put(
        `/api/pages/${selectedPageId}`,
        {
          id: selectedPageId,
          title,
          content,
          parentPageId,
          displayOrder,
          associatedQuestions: questionsPayload,
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

      <label htmlFor="pageSelect" className="page-label">
        Vælg Side
      </label>
      <select id="pageSelect" value={selectedPageId ?? ""} onChange={(e) => setSelectedPageId(Number(e.target.value))} className="page-input">
        <option value="">-- Vælg side --</option>
        {pages.map((p) => (
          <option key={p.id} value={p.id}>
            {p.title}
          </option>
        ))}
      </select>

      {selectedPageId && (
        <form onSubmit={handleSubmit}>
          <label htmlFor="titleInput" className="page-label">
            Titel <span style={{ color: "red" }}>*</span>
          </label>
          <input id="titleInput" type="text" value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Titel" className="page-input" />

          <label htmlFor="contentInput" className="page-label">
            Indhold (Markdown)
          </label>
          <textarea
            id="contentInput"
            value={content}
            onChange={(e) => setContent(e.target.value)}
            rows={6}
            placeholder="Markdown indhold"
            className="page-form page-textarea"
          />

          <label htmlFor="parentPageSelect" className="page-label">
            Overordnet side
          </label>
          <select
            id="parentPageSelect"
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

          <label htmlFor="displayOrderInput" className="page-label">
            Visningsrækkefølge
          </label>
          <input
            id="displayOrderInput"
            type="number"
            value={displayOrder}
            onChange={(e) => setDisplayOrder(Number(e.target.value))}
            className="page-input"
          />

          <div className="questions-admin-section">
            <h2>Spørgsmål tilknyttet siden</h2>
            {questions.map((q, qIndex) => (
              <div key={q.id} className="question-admin-block">
                <label htmlFor={`question-text-${q.id}`} className="page-label">
                  Spørgsmålstekst
                </label>
                <textarea
                  id={`question-text-${q.id}`}
                  value={q.questionText}
                  onChange={(e) => handleQuestionTextChange(qIndex, e.target.value)}
                  placeholder="Indtast spørgsmål"
                  className="page-input page-textarea"
                  rows={2}
                />
                <h4>Svarsmuligheder</h4>
                {q.options.map((opt, oIndex) => (
                  <div key={opt.id} className="answer-option-admin-block">
                    <input
                      type="text"
                      value={opt.optionText}
                      onChange={(e) => handleOptionTextChange(qIndex, oIndex, e.target.value)}
                      placeholder="Svarsmulighed"
                      className="page-input"
                    />
                    <label className="correct-answer-label">
                      <input type="checkbox" checked={opt.isCorrect} onChange={(e) => handleOptionCorrectChange(qIndex, oIndex, e.target.checked)} />
                      <span>Korrekt?</span>
                    </label>
                  </div>
                ))}
                <div className="option-management-controls">
                  <button type="button" onClick={() => handleAddOption(qIndex)} className="add-btn">
                    Tilføj Svarsmulighed
                  </button>
                  <button type="button" onClick={() => handleRemoveLastOption(qIndex)} className="remove-btn">
                    Fjern Sidste Svar
                  </button>
                </div>
                <button type="button" onClick={() => handleRemoveQuestion(qIndex)} className="remove-btn question-remove-btn">
                  Fjern Spørgsmål
                </button>
              </div>
            ))}
            <button type="button" onClick={handleAddQuestion} className="add-btn add-question-btn">
              Tilføj Spørgsmål
            </button>
          </div>

          <button type="submit" className="page-btn save-page-btn">
            Gem Ændringer
          </button>
        </form>
      )}
    </div>
  );
}
