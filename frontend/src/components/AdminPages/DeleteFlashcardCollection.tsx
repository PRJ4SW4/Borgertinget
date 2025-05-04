import { useEffect, useState } from "react";
import axios from "axios";
import {
  FlashcardDto,
  FlashcardCollectionDetailDto,
  FlashcardContentType,
} from "../../types/flashcardTypes";
import "./DeleteFlashcardCollection.css";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";

export default function DeleteFlashcardCollection() {
  const [titles, setTitles] = useState<string[]>([]);

  // Load all titles
  useEffect(() => {
    const fetchTitles = async () => {
      try {
        const res = await axios.get<string[]>(
          "/api/administrator/GetAllFlashcardCollectionTitles"
        );
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
        `/api/administrator/GetFlashcardCollectionByTitle?title=${encodeURIComponent(
          title
        )}`
      );

      const response = await axios.delete(
        `/api/administrator/DeleteFlashcardCollection?collectionId=${res.data.collectionId}`
      );
      console.log("Collection Deleted", response.data);

      alert("Flashcard serie slettet!");

      setTitles((st) => st.filter((t) => t !== title));

    } catch (err) {
      console.error(err);
    }
  };

  return (
    <div className="container">
        <div><img src={BorgertingetIcon} className='Borgertinget-Icon'/></div>
        <div className='top-red-line'></div>
        <h1>Slet Flashcard Serie</h1>


        {/* List all Titles */}
        <div className="flashcard-titles">
            <h2>Flashcard serier:</h2>
            {titles.map((title, idx) => (
              <button 
              key={idx} 
              onClick={() => DeleteCollection(title)}
              className="flashcard-title-button">
                {title}
              </button>
            ))}
          </div>
    </div>
  );
}
