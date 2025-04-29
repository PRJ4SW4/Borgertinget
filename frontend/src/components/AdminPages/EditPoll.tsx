import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import "./ChangePolls.css";

interface PollSummary {
  id: number;
  question: string;
}

interface PollOption {
  id: number;
  optionText: string;
}

interface Question {
  id: number;
  question: string;
  options: PollOption[];
}

interface PollDetails {
  id: number;
  question: string;
  options: { id: number; optionText: string }[];
  endedAt: string | null;
  politicianId: string | null;
}

interface Politician {
  id: string;
  navn: string;
}

export default function EditPoll() {
  const [polls, setPolls] = useState<PollSummary[]>([]);
  const [selectedPollId, setSelectedPollId] = useState<number | null>(null);
  const [feedText, setFeedText] = useState("");
  const [questions, setQuestions] = useState<Question[]>([]);
  const [politicians, setPoliticians] = useState<Politician[]>([]);
  const [selectedPoliticianId, setSelectedPoliticianId] = useState<string | null>(null);
  const [endDate, setEndDate] = useState<string | null>(null);
  const navigate = useNavigate();

  // Fetch all polls once
  useEffect(() => {
    async function fetchPolls() {
      const token = localStorage.getItem("jwt");
      if (!token) {
        alert("Du er ikke logget ind.");
        return;
      }

      try {
        const response = await axios.get("/api/polls", {
          headers: { Authorization: `Bearer ${token}` },
        });
        setPolls(response.data);
      } catch (error) {
        console.error("Failed to fetch polls", error);
      }
    }

    fetchPolls();
  }, []);

  // Fetch all politicians once
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

  // Fetch selected poll details
  useEffect(() => {
    if (selectedPollId === null) return;

    async function fetchPollDetails() {
      const token = localStorage.getItem("jwt");
      if (!token) {
        alert("Du er ikke logget ind.");
        return;
      }

      try {
        const response = await axios.get(`/api/polls/${selectedPollId}`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        const pollData: PollDetails = response.data;

        setFeedText(""); // If not used
        setQuestions([
          {
            id: pollData.id,
            question: pollData.question,
            options: pollData.options.map((o) => ({
              id: o.id,
              optionText: o.optionText,
            })),
          },
        ]);
        setEndDate(pollData.endedAt ? pollData.endedAt.split("T")[0] : null);
        setSelectedPoliticianId(pollData.politicianId ? String(pollData.politicianId) : null);
      } catch (error) {
        console.error("Failed to fetch poll details", error);
      }
    }
    fetchPollDetails();
  }, [selectedPollId]);

  const handleQuestionChange = (index: number, value: string) => {
    const newQuestions = [...questions];
    newQuestions[index].question = value;
    setQuestions(newQuestions);
  };

  const handleOptionChange = (qIndex: number, oIndex: number, value: string) => {
    const newQuestions = [...questions];
    newQuestions[qIndex].options[oIndex].optionText = value;
    setQuestions(newQuestions);
  };

  const addOption = (qIndex: number) => {
    const newQuestions = [...questions];
    if (newQuestions[qIndex].options.length < 4) {
      newQuestions[qIndex].options.push({ id: 0, optionText: "" });
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

    const token = localStorage.getItem("jwt");
    if (!token) {
      alert("Du er ikke logget ind.");
      return;
    }

    if (!selectedPoliticianId) {
      alert("Du skal vælge en politiker.");
      return;
    }

    const payload = {
      question: questions[0]?.question || "",
      options: questions[0]?.options.map((o) => o.optionText).filter((o) => o.trim() !== "") || [],
      politicianTwitterId: selectedPoliticianId,
      endedAt: endDate ? new Date(endDate).toISOString() : null,
    };

    try {
      await axios.put(`/api/polls/${selectedPollId}`, payload, {
        headers: { Authorization: `Bearer ${token}` },
      });
      navigate("/admin/polls");
    } catch (error) {
      console.error("Failed to update poll", error);
    }
  };

  return (
    <div className="add-poll-container">
      <h1 className="add-poll-title">Rediger Poll</h1>
      <p className="add-poll-subtitle">Vælg en poll for at redigere</p>

      <div className="add-poll-section">
        <label className="add-poll-label">Vælg Poll</label>
        <select className="add-poll-input" value={selectedPollId ?? ""} onChange={(e) => setSelectedPollId(Number(e.target.value))}>
          <option value="">-- Vælg en poll --</option>
          {polls.map((poll) => (
            <option key={poll.id} value={poll.id}>
              {poll.question}
            </option>
          ))}
        </select>
      </div>

      {selectedPollId && (
        <form onSubmit={handleSubmit} className="add-poll-form">
          <div className="add-poll-section">
            <label className="add-poll-label">Vælg Politiker</label>
            <select
              className="add-poll-input"
              value={selectedPoliticianId ?? ""}
              onChange={(e) => setSelectedPoliticianId(String(e.target.value))}
              required>
              <option value="">-- Vælg en politiker --</option>
              {politicians.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.navn}
                </option>
              ))}
            </select>
          </div>

          {questions.map((q, qIndex) => (
            <div key={q.id || qIndex} className="add-poll-section">
              <label className="add-poll-label">{`Spørgsmål ${qIndex + 1}`}</label>
              <input
                type="text"
                value={q.question}
                onChange={(e) => handleQuestionChange(qIndex, e.target.value)}
                className="add-poll-input"
                placeholder={`Skriv spørgsmål ${qIndex + 1} her...`}
                required
              />

              <div className="add-poll-option-group">
                {q.options.map((option, oIndex) => (
                  <div key={option.id || oIndex} className="add-poll-option">
                    <input
                      type="text"
                      value={option.optionText}
                      onChange={(e) => handleOptionChange(qIndex, oIndex, e.target.value)}
                      className="add-poll-input"
                      placeholder={`Svarmulighed ${qIndex + 1}.${oIndex + 1}`}
                      required
                    />
                  </div>
                ))}
              </div>

              <div className="add-poll-buttons">
                <button type="button" onClick={() => addOption(qIndex)} className="add-poll-add-btn" disabled={q.options.length >= 4}>
                  Tilføj Svarmulighed
                </button>
                <button type="button" onClick={() => removeOption(qIndex)} className="add-poll-remove-btn" disabled={q.options.length <= 2}>
                  Fjern Svarmulighed
                </button>
              </div>
            </div>
          ))}

          <div className="add-poll-section">
            <label className="add-poll-label">Slutdato (valgfri)</label>
            <input type="date" value={endDate ?? ""} onChange={(e) => setEndDate(e.target.value)} className="add-poll-input" />
          </div>

          <div className="add-poll-buttons">
            <button type="submit" className="add-poll-submit-btn">
              Opdater Poll
            </button>
          </div>
        </form>
      )}
    </div>
  );
}
