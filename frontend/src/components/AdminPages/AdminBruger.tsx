import { useState } from "react";
import { useLocation } from "react-router-dom";
import axios from "axios";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import "./AdminBruger.css";
import BackButton from "../Button/backbutton";

// Allow an admin to edit a users username
export default function AdminBruger() {
  const [oldUsername, setOldUsername] = useState<string>("");
  const [newUsername, setNewUsername] = useState<string>("");
  const location = useLocation();
  // Back button
  const matchProp = { path: location.pathname };

  // Edit username
  const editUsername = async () => {
    if (!oldUsername.trim() || !newUsername.trim()) {
      alert("Udfyld både det gamle og det nye brugernavn.");
      return; 
    }

    try {
      // Lookup  the user ID by the old username
      const getRes = await axios.get<number>(`/api/administrator/username`, {
        params: { username: oldUsername },
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("jwt")}`,
        },
      });
      const userId = getRes.data;

      // Call put to update username
      await axios.put(
        `/api/administrator/${userId}`,
        { userName: newUsername },
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("jwt")}`,
          },
        }
      );

      alert("Brugernavn opdateret!");

      // Reset inputs
      setOldUsername("");
      setNewUsername("");
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        if (err.response?.status === 404) {
          // If user is not found

          alert("Fejl: Bruger ikke fundet");
        } else {
          // Error updating username
          alert("Fejl under opdatering af brugernavn");
        }
      } else {
        // Unkown issue
        console.error("Ukendt fejl:", err);
        alert("Uventet fejl opstod.");
      }
    }
  };

  return (
    <div className="admin-bruger-container">
      <div style={{ position: "relative" }}>
        {" "}
        <img
          src={BorgertingetIcon}
          className="Borgertinget-Icon"
          alt="Borgertinget Icon"
        />
        <div style={{ position: "absolute", top: "10px", left: "10px" }}>
          {/* --- Back button ---*/}
          <BackButton match={matchProp} destination="admin" />
        </div>
      </div>
      <div className="top-red-line"></div>

      <h1>Ændre Brugernavn</h1>

      <div className="input-group">
        {/* --- input old username ---*/}
        <input
          type="text"
          required
          placeholder="Gammel brugernavn"
          value={oldUsername}
          onChange={(e) => setOldUsername(e.target.value)}
        />

        {/* --- Input new username ---*/}
        <input
          type="text"
          required
          placeholder="Ny brugernavn"
          value={newUsername}
          onChange={(e) => setNewUsername(e.target.value)}
        />
      </div>

      {/* --- Edit username button ---*/}
      <button className="Button" onClick={editUsername}>
        Ændrer brugernavn
      </button>
    </div>
  );
}
