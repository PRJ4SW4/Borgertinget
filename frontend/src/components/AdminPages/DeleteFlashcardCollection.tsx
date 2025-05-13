import { useEffect, useState } from "react";
import axios from "axios";
import { useLocation } from "react-router-dom";
import { FlashcardCollectionDetailDto } from "../../types/flashcardTypes";
import "./DeleteFlashcardCollection.css";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import BackButton from "../Button/backbutton";

export default function DeleteFlashcardCollection() {
  const [titles, setTitles] = useState<string[]>([]);
  const location = useLocation();

  const matchProp = { path: location.pathname };

  // Load all titles
  useEffect(() => {
    const fetchTitles = async () => {
      try {
        const res = await axios.get<string[]>("/api/administrator/GetAllFlashcardCollectionTitles", {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("jwt")}`,
          },
        });
        setTitles(res.data);
      } catch (err) {
        console.error(err);
      }
    };

    fetchTitles();
  }, []);

  // Fetch collection when a title is clicked
  const DeleteCollection = async (title: string) => {
    try {
      const res = await axios.get<FlashcardCollectionDetailDto>(
        `/api/administrator/GetFlashcardCollectionByTitle?title=${encodeURIComponent(title)}`,
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("jwt")}`,
          },
        }
      );

      const response = await axios.delete(`/api/administrator/DeleteFlashcardCollection?collectionId=${res.data.collectionId}`, {
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("jwt")}`,
        },
      });
      console.log("Collection Deleted", response.data);

      alert("Flashcard serie slettet!");

      setTitles((st) => st.filter((t) => t !== title));
    } catch (err) {
      console.error(err);
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
      <h1>Slet Flashcard Serie</h1>

      {/* List all Titles */}
      <div className="flashcard-titles">
        <h2>Flashcard serier:</h2>
        {titles.map((title, idx) => (
          <button key={idx} onClick={() => DeleteCollection(title)} className="flashcard-title-button">
            {title}
          </button>
        ))}
      </div>
    </div>
  );
}
