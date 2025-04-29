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

interface Politician {
  id: string;
  navn: string;
}

export default function DeletePoll() {
  const [polls, setPolls] = useState<PollSummary[]>([]);
  const [selectedPollId, setSelectedPollId] = useState<number | null>(null);
  const [questions, setQuestions] = useState<Question[]>([]);
  const [politicians, setPoliticians] = useState<Politician[]>([]);
  const [selectedPoliticianId, setSelectedPoliticianId] = useState<string | null>(null);
  const [endDate, setEndDate] = useState<string | null>(null);
  const navigate = useNavigate();

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
        const pollData = response.data;
        setQuestions([
          {
            id: pollData.id,
            question: pollData.question,
            options: pollData.options.map((o: any) => ({
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

  const handleDelete = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedPollId) return;

    const confirmDelete = window.confirm("Er du sikker på, at du vil slette denne poll?");
    if (!confirmDelete) return;

    const token = localStorage.getItem("jwt");
    if (!token) {
      alert("Du er ikke logget ind.");
      return;
    }

    try {
      await axios.delete(`/api/polls/${selectedPollId}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      navigate("/admin/polls");
    } catch (error) {
      console.error("Failed to delete poll", error);
    }
  };

  return (
    <div className="add-poll-container">
      <h1 className="add-poll-title">Slet Poll</h1>
      <p className="add-poll-subtitle">Vælg en poll for at slette</p>

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
        <form onSubmit={handleDelete} className="add-poll-form">
          <div className="add-poll-section">
            <label className="add-poll-label">Vælg Politiker</label>
            <select className="add-poll-input" value={selectedPoliticianId ?? ""} disabled>
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
              <input type="text" value={q.question} disabled className="add-poll-input" placeholder={`Skriv spørgsmål ${qIndex + 1} her...`} />

              <div className="add-poll-option-group">
                {q.options.map((option, oIndex) => (
                  <div key={option.id || oIndex} className="add-poll-option">
                    <input
                      type="text"
                      value={option.optionText}
                      disabled
                      className="add-poll-input"
                      placeholder={`Svarmulighed ${qIndex + 1}.${oIndex + 1}`}
                    />
                  </div>
                ))}
              </div>
            </div>
          ))}

          <div className="add-poll-section">
            <label className="add-poll-label">Slutdato</label>
            <input type="date" value={endDate ?? ""} disabled className="add-poll-input" />
          </div>

          <div className="add-poll-buttons">
            <button type="submit" className="add-poll-submit-btn" style={{ backgroundColor: "#991b1b" }}>
              Slet Poll
            </button>
          </div>
        </form>
      )}
    </div>
  );
}
