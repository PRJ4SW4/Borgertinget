import { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import axios from "axios";
import "./ChangePolls.css";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import BackButton from "../Button/backbutton";
interface Politician {
  id: string;
  navn: string; // Using the correct field 'navn' from API response
}

interface Question {
  question: string;
  options: string[];
}

export default function AddPolls() {
  const [questions, setQuestions] = useState<Question[]>([{ question: "", options: ["", ""] }]);
  const [politicians, setPoliticians] = useState<Politician[]>([]);
  const [selectedPoliticianId, setSelectedPoliticianId] = useState<string | null>(null);
  // const [twitterId, setTwitterId] = useState<number | null>(null); Uncomment this line when TwitterUserIds are available
  const [endDate, setEndDate] = useState<string | null>(null);
  const navigate = useNavigate();
  const location = useLocation();
  const matchProp = { path: location.pathname };

  useEffect(() => {
    async function fetchPoliticians() {
      const token = localStorage.getItem("jwt");
      if (!token) {
        alert("Du er ikke logget ind.");
        return;
      }

      try {
        const response = await axios.get("/api/aktor/all", {
          headers: { Authorization: `Bearer ${token}` },
        });
        setPoliticians(response.data);
      } catch (error) {
        console.error("Failed to fetch politicians", error);
      }
    }

    fetchPoliticians();
  }, []);

  /*useEffect(() => {
    const fetchTwitterId = async () => {
      if (!selectedPoliticianId) return;

      try {
        const token = localStorage.getItem("jwt");
        const response = await axios.get(`/api/subscription/lookup/politicianTwitterId?aktorId=${selectedPoliticianId}`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        setTwitterId(response.data.politicianTwitterId);
      } catch (error) {
        console.error("Could not fetch politicianTwitterId", error);
        setTwitterId(null);
      }
    };

    fetchTwitterId();
  }, [selectedPoliticianId]);
*/
  const handleQuestionChange = (index: number, value: string) => {
    const newQuestions = [...questions];
    newQuestions[index].question = value;
    setQuestions(newQuestions);
  };

  const handleOptionChange = (qIndex: number, oIndex: number, value: string) => {
    const newQuestions = [...questions];
    newQuestions[qIndex].options[oIndex] = value;
    setQuestions(newQuestions);
  };

  const addOption = (qIndex: number) => {
    const newQuestions = [...questions];
    if (newQuestions[qIndex].options.length < 4) {
      newQuestions[qIndex].options.push("");
      setQuestions(newQuestions);
    }
  };

  const removeOption = (qIndex: number) => {
    const newQuestions = [...questions];
    if (newQuestions[qIndex].options.length > 2) {
      newQuestions[qIndex].options.pop();
      setQuestions(newQuestions);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedPoliticianId) {
      alert("Du skal vælge en politiker.");
      return;
    }

    if (questions.length !== 1) {
      alert("Du kan kun oprette ét spørgsmål ad gangen.");
      return;
    }

    const token = localStorage.getItem("jwt");
    if (!token) {
      alert("Du er ikke logget ind.");
      return;
    }

    const payload = {
      question: questions[0].question,
      options: questions[0].options.filter((o) => o.trim() !== ""),
      politicianTwitterId: 3, // Troels Lunds Twitter ID
      // politicianTwitterId: twitterId, // Uncomment this line when TwitterUserIds are available
      endedAt: endDate ? new Date(endDate).toISOString() : null,
    };

    try {
      await axios.post("/api/polls", payload, {
        headers: { Authorization: `Bearer ${token}` },
      });
      navigate("/admin/polls");
    } catch (error) {
      console.error("Failed to create poll", error);
      window.alert("Failed to create poll. Please try again."); // Added alert
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
      <h1 className="add-poll-title">Opret en Poll</h1>
      <p className="add-poll-subtitle">Vælg politiker og lav dit spørgsmål</p>

      <form onSubmit={handleSubmit} className="add-poll-form">
        {/* Politician Selection */}
        <div className="add-poll-section">
          <label className="add-poll-label" htmlFor="politicianSelect">
            Vælg Politiker <span style={{ color: "red" }}>*</span>
          </label>
          <select
            id="politicianSelect"
            className="add-poll-input"
            value={selectedPoliticianId ?? ""}
            onChange={(e) => setSelectedPoliticianId(String(e.target.value))}>
            <option value="">-- Vælg en politiker --</option>
            {politicians.map((p) => (
              <option key={p.id} value={p.id}>
                {p.navn}
              </option>
            ))}
          </select>
        </div>

        {/* Question Input */}
        {questions.map((q, qIndex) => (
          <div key={qIndex} className="add-poll-section">
            <label className="add-poll-label" htmlFor="questionInput">
              Spørgsmål <span style={{ color: "red" }}>*</span>
            </label>
            <input
              id="questionInput"
              type="text"
              value={q.question}
              onChange={(e) => handleQuestionChange(qIndex, e.target.value)}
              className="add-poll-input"
              placeholder={`Skriv spørgsmålet her...`}
              required
            />

            <div className="add-poll-option-group">
              {q.options.map((option, oIndex) => (
                <div key={oIndex} className="add-poll-option">
                  <input
                    type="text"
                    value={option}
                    onChange={(e) => handleOptionChange(qIndex, oIndex, e.target.value)}
                    className="add-poll-input"
                    placeholder={`Svarmulighed ${oIndex + 1}`}
                    required
                  />
                </div>
              ))}
            </div>

            {/* Add and Remove Options */}
            <div className="add-poll-buttons">
              <button type="button" onClick={() => addOption(qIndex)} className="add-poll-add-btn" disabled={questions[qIndex].options.length >= 4}>
                Tilføj Svarmulighed
              </button>

              <button
                type="button"
                onClick={() => removeOption(qIndex)}
                className="add-poll-remove-btn"
                disabled={questions[qIndex].options.length <= 2}>
                Fjern Svarmulighed
              </button>
            </div>
          </div>
        ))}

        {/* End Date */}
        <div className="add-poll-section">
          <label className="add-poll-label" htmlFor="endDateSelect">
            Slutdato (valgfri)
          </label>
          <input id="endDateSelect" type="date" value={endDate ?? ""} onChange={(e) => setEndDate(e.target.value)} className="add-poll-input" />
        </div>

        {/* Submit */}
        <div className="add-poll-buttons">
          <button type="submit" className="add-poll-submit-btn">
            Opret Poll
          </button>
        </div>
      </form>
    </div>
  );
}
