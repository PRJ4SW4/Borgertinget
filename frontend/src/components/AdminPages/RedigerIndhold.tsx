import { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import axios from "axios";
import "./ChangeInhold.css";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import BackButton from "../Button/backbutton";

interface Party {
  partyId: number;
  partyName: string;
  partyProgram: string;
  history: string;
  politics: string;
}

export default function RedigerInhold() {
  const [parties, setParties] = useState<Party[]>([]);
  const [selectedParty, setSelectedParty] = useState<Party | null>(null);
  const [editData, setEditData] = useState({
    partyProgram: "",
    history: "",
    politics: "",
  });
  const location = useLocation();

  const matchProp = { path: location.pathname };

  useEffect(() => {
    async function fetchParties() {
      const token = localStorage.getItem("jwt");
      if (!token) {
        alert("Du er ikke logget ind.");
        return;
      }

      try {
        const response = await axios.get("/api/Party/Parties", {
          headers: { Authorization: `Bearer ${token}` },
        });
        setParties(response.data);
      } catch (error) {
        console.error("Kunne ikke hente partier", error);
      }
    }

    fetchParties();
  }, []);

  const handleSelect = (party: Party) => {
    setSelectedParty(party);
    setEditData({
      partyProgram: party.partyProgram || "",
      history: party.history || "",
      politics: party.politics || "",
    });
    console.log("Selected party:", party);
  };

  const handleChange = (field: string, value: string) => {
    setEditData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedParty || !selectedParty.partyId) {
      alert("Parti er ikke korrekt valgt.");
      return;
    }

    const token = localStorage.getItem("jwt");
    if (!token) {
      alert("Du er ikke logget ind.");
      return;
    }

    try {
      await axios.put(`/api/Party/${selectedParty.partyId}`, editData, {
        headers: { Authorization: `Bearer ${token}` },
      });

      alert("Parti opdateret!");
    } catch (error) {
      console.error("Kunne ikke opdatere parti", error);
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

      <h1>Rediger Indhold for Partier</h1>
      <p className="edit-inhold-subtitle">Klik på et parti for at redigere deres indhold</p>

      {selectedParty && <h2 className="edit-inhold-selected">Valgt parti: {selectedParty.partyName}</h2>}

      <div className="edit-inhold-button-group">
        {!selectedParty &&
          parties.map((party) => (
            <button key={party.partyId} className="edit-inhold-submit-btn" onClick={() => handleSelect(party)}>
              {party.partyName}
            </button>
          ))}

        {selectedParty && (
          <button className="edit-inhold-submit-btn" onClick={() => setSelectedParty(null)}>
            Vælg andet parti
          </button>
        )}
      </div>

      {selectedParty && (
        <form onSubmit={handleSubmit} className="edit-inhold-form">
          <div className="edit-inhold-section">
            <label htmlFor="partyProgram" className="edit-inhold-label">
              Partiprogram <span style={{ color: "red" }}>*</span>
            </label>
            <textarea
              id="partyProgram"
              className="edit-inhold-input"
              value={editData.partyProgram}
              onChange={(e) => handleChange("partyProgram", e.target.value)}
              rows={4}
              required
            />
          </div>

          <div className="edit-inhold-section">
            <label htmlFor="politics" className="edit-inhold-label">
              Politik <span style={{ color: "red" }}>*</span>
            </label>
            <textarea
              id="politics"
              className="edit-inhold-input"
              value={editData.politics}
              onChange={(e) => handleChange("politics", e.target.value)}
              rows={4}
              required
            />
          </div>

          <div className="edit-inhold-section">
            <label htmlFor="history" className="edit-inhold-label">
              Historie <span style={{ color: "red" }}>*</span>
            </label>
            <textarea
              id="history"
              className="edit-inhold-input"
              value={editData.history}
              onChange={(e) => handleChange("history", e.target.value)}
              rows={4}
              required
            />
          </div>

          <div className="edit-inhold-buttons">
            <button type="submit" className="edit-inhold-submit-btn">
              Opdater Parti
            </button>
          </div>
        </form>
      )}
    </div>
  );
}
