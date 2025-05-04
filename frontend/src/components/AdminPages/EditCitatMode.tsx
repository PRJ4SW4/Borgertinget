import { useEffect, useState } from "react";
import axios from "axios";
import "./EditCitatMode.css";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import { EditQuoteDTO } from "../../types/polidleTypes";

export default function EditCitatMode() {
  const [quotes, setQuotes] = useState<EditQuoteDTO[]>([]);
  const [selectedQuote, setSelectedQuote] = useState<EditQuoteDTO | null>(null);
  const [newText, setNewText] = useState<string>("");

  // Load all Quotes
  useEffect(() => {
    const fetchQuotes = async () => {
      try {
        const res = await axios.get<EditQuoteDTO[]>(
          "/api/administrator/GetAllQuotes"
        );
        setQuotes(res.data);
      } catch (err) {
        console.error(err);
      }
    };

    fetchQuotes();
  }, []);

  // Fetch Quote when a Quote is clicked
  const fetchQuote = async (quoteId: number) => {
    try {
      const res = await axios.get<EditQuoteDTO>(
        `/api/administrator/GetQuoteById?quoteId=${quoteId}`
      );
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
      `/api/administrator/EditQuote`, // Action name
      {}, // No body
      {
        params: {
          quoteId: selectedQuote.quoteId,
          quoteText: newText,
        },
      }
    );

    alert("Citat opdateret!");

    // Update local list of Quotes without using the GET
    setQuotes((qs) =>
      qs.map((q) =>
        q.quoteId === selectedQuote.quoteId ? { ...q, quoteText: newText } : q
      )
    );

    setSelectedQuote(null);
    setNewText("");
  };

  return (
    <div className="container">

    <div><img src={BorgertingetIcon} className='Borgertinget-Icon'></img></div>
    <div className='top-red-line'></div>

      <h1>Rediger Citat‑mode</h1>

      <ul className="quote-list">
        {quotes.map((q) => (
          <li key={q.quoteId}>
            <button
              className="quote-button"
              onClick={() => fetchQuote(q.quoteId)}
            >
              {q.quoteText}
            </button>
          </li>
        ))}
      </ul>

      {selectedQuote && (
        <div className="editor">
          <span className="quote-badge">ID #{selectedQuote.quoteId}</span>
          <br/>

          <textarea
            value={newText}
            onChange={(e) => setNewText(e.target.value)}
          />

          <br />
          <button className="save-button" onClick={saveQuote}>
            Gem
          </button>
          <button
            className="cancel-button"
            onClick={() => setSelectedQuote(null)}
          >
            Annullér
          </button>
        </div>
      )}
    </div>
  );
}
