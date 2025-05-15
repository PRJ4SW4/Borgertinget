import { useEffect, useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { fetchPagesStructure } from "../../services/ApiService";
import type { PageSummaryDto } from "../../types/pageTypes";
import "./ChangeLearningPage.css";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import BackButton from "../Button/backbutton";

// Frontend state types for question/answer editing
interface AnswerOptionFormState {
  id: number; // Client-side temporary ID, or 0 for new backend item
  optionText: string;
  isCorrect: boolean;
  displayOrder: number;
}

interface QuestionFormState {
  id: number; // Client-side temporary ID, or 0 for new backend item
  questionText: string;
  options: AnswerOptionFormState[];
}

export default function AddLearningPage() {
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [parentId, setParentId] = useState<number | null>(null);
  const [pages, setPages] = useState<PageSummaryDto[]>([]);
  const [questions, setQuestions] = useState<QuestionFormState[]>([]);
  const navigate = useNavigate();
  const location = useLocation();

  // For generating unique client-side IDs for new items
  const [nextQuestionTempId, setNextQuestionTempId] = useState(-1);
  const [nextOptionTempId, setNextOptionTempId] = useState(-1);

  const matchProp = { path: location.pathname }; // For BackButton
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

  const handleAddQuestion = () => {
    setQuestions([
      ...questions,
      {
        id: nextQuestionTempId, // Use temporary client-side ID
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
    const newQuestions = [...questions];
    newQuestions[qIndex].questionText = text;
    setQuestions(newQuestions);
  };

  const handleRemoveQuestion = (qIndex: number) => {
    setQuestions(questions.filter((_, index) => index !== qIndex));
  };

  const handleAddOption = (qIndex: number) => {
    const newQuestions = [...questions];
    newQuestions[qIndex].options.push({
      id: nextOptionTempId, // Use temporary client-side ID
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
      newQuestions[qIndex].options.pop(); // Removes the last element
      setQuestions(newQuestions);
    } else {
      alert("Der er ingen svarmuligheder at fjerne.");
    }
  };

  const handleSubmit = async () => {
    if (!title.trim()) {
      alert("Titel må ikke være tom.");
      return;
    }

    // Prepare questions for payload, ensuring IDs are 0 for new items
    const questionsPayload = questions.map((q) => ({
      ...q,
      id: 0, // New questions always have id 0 for backend
      options: q.options.map((opt) => ({
        ...opt,
        id: 0, // New options always have id 0 for backend
      })),
    }));

    const payload = {
      title,
      content,
      parentPageId: parentId,
      displayOrder: 1, // Or make this configurable
      associatedQuestions: questionsPayload,
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
        navigate("/admin/Laering");
      } else {
        const errorData = await response.json();
        console.error("Serverfejl:", errorData);
        let errorMessage = "Fejl under oprettelse af side.";
        if (errorData && errorData.errors) {
          errorMessage += " Detaljer: " + JSON.stringify(errorData.errors);
        } else if (errorData && errorData.message) {
          errorMessage += " Detaljer: " + errorData.message;
        } else {
          errorMessage += " Status: " + response.status;
        }
        alert(errorMessage);
      }
    } catch (err) {
      console.error("Netværksfejl:", err);
      alert("Netværksfejl ved oprettelse af side.");
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
      <h1>Opret Læringsside</h1>
      <label htmlFor="titleInput" className="page-label">
        Titel <span style={{ color: "red" }}>*</span>
      </label>
      <input id="titleInput" type="text" placeholder="Titel" value={title} onChange={(e) => setTitle(e.target.value)} className="page-input" />

      <label htmlFor="contentInput" className="page-label">
        Indhold (Markdown) <span style={{ color: "red" }}>*</span>
      </label>
      <textarea
        id="contentInput"
        className="page-form page-textarea" // Added page-textarea for specific styling
        placeholder="Indhold (Markdown understøttet)"
        value={content}
        onChange={(e) => setContent(e.target.value)}
        rows={6}
      />

      <label htmlFor="parentPageSelect" className="page-label">
        Overordnet side
      </label>
      <select
        id="parentPageSelect"
        value={parentId ?? ""}
        onChange={(e) => {
          const value = e.target.value;
          setParentId(value === "" ? null : Number(value));
        }}
        className="page-input">
        <option value="">(Ingen overordnet side)</option>
        {pages.map((page) => (
          <option key={page.id} value={page.id}>
            {page.title}
          </option>
        ))}
      </select>

      {/* Questions Section */}
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

      <button onClick={handleSubmit} className="page-btn save-page-btn">
        Gem Side
      </button>
    </div>
  );
}
