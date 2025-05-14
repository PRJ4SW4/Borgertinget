import { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import axios from "axios";
import "./EditCitatMode.css";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import BackButton from "../Button/backbutton";
import { EditQuoteDTO } from "../../types/PolidleTypes";

export default function EditCitatMode() {
  const [quotes, setQuotes] = useState<EditQuoteDTO[]>([]);
  const [selectedQuote, setSelectedQuote] = useState<EditQuoteDTO | null>(null);
  const [newText, setNewText] = useState<string>("");
  const location = useLocation();

  const matchProp = { path: location.pathname };

  // Load all Quotes
  useEffect(() => {
    const fetchAllQuotes = async () => {
      try {
        const res = await axios.get<EditQuoteDTO[]>("/api/administrator/GetAllQuotes", {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("jwt")}`,
          },
        });
        setQuotes(res.data);
      } catch (err) {
        console.error(err);
      }
    };

    fetchAllQuotes();
  }, []);

  // Fetch Quote when a Quote is clicked
  const fetchQuote = async (quoteId: number) => {
    try {
      const res = await axios.get<EditQuoteDTO>(`/api/administrator/GetQuoteById?quoteId=${quoteId}`, {
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("jwt")}`,
        },
      });
      setSelectedQuote(res.data);
      setNewText(res.data.quoteText);
    } catch (err) {
      console.error(err);
    }
  };

  // Save Quote
  const saveQuote = async () => {
    if (!selectedQuote) return;

    await axios.put(
      "/api/administrator/EditQuote",
      {
        quoteId: selectedQuote.quoteId,
        quoteText: newText,
      },
      {
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("jwt")}`,
        },
      }
    );

    alert("Citatet er opdateret!");

    // Update local list of Quotes without using the GET
    setQuotes((qs) => qs.map((q) => (q.quoteId === selectedQuote.quoteId ? { ...q, quoteText: newText } : q)));

    setSelectedQuote(null);
    setNewText("");
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

      <h1>Rediger Citat‑mode</h1>

      {/* Conditional rendering for quote list OR "Choose another quote" button */}
      {!selectedQuote ? (
        <ul className="quote-list">
          {quotes.map((q) => (
            <li key={q.quoteId}>
              <button className="quote-button" onClick={() => fetchQuote(q.quoteId)}>
                {q.quoteText}
              </button>
            </li>
          ))}
        </ul>
      ) : (
        <button
          className="cancel-button"
          style={{ marginBottom: "1rem" }}
          onClick={() => {
            setSelectedQuote(null);
            setNewText(""); // Clear the text area when deselecting
          }}>
          Vælg et andet citat
        </button>
      )}

      {selectedQuote && (
        <div className="editor">
          <span className="quote-badge">ID #{selectedQuote.quoteId}</span>
          <br />

          <label htmlFor="editQuoteTextarea">Rediger citat:</label>
          <textarea id="editQuoteTextarea" value={newText} onChange={(e) => setNewText(e.target.value)} />

          <br />
          <button className="save-button" onClick={saveQuote}>
            Gem
          </button>
        </div>
      )}
    </div>
  );
}
