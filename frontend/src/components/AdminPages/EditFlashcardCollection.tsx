import { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";
import axios from "axios";
import { FlashcardDto, FlashcardCollectionDetailDto } from "../../types/flashcardTypes";
import "./EditFlashcardCollection.css";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import BackButton from "../Button/backbutton"; // Import BackButton

// Allow Admin to edit an existing flashcard
export default function EditFlashcardCollection() {
  const [titles, setTitles] = useState<string[]>([]);
  const [selectedTitle, setSelectedTitle] = useState<string>("");
  const [collection, setCollection] = useState<FlashcardCollectionDetailDto | null>(null);
  const location = useLocation();

  // Back button
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
  const fetchCollection = async (title: string) => {
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

      setCollection(res.data);
      setSelectedTitle(title);

    } catch (err) {
      console.error(err);
    }
  };

  // keyof for type safety
  // update one specific property on
  // one specific flashcard in the collection’s array, leaving everything else untouched.
  // index - index of the flashcard
  // field - The type of flashcard being changed (e.g. "frontText")
  // value - value inside the flashcard
  const handleFlashcardChange = (index: number, field: keyof FlashcardDto, value: string) => {
    if (!collection) return;

    // Clone existing collection
    const updatedCollection = { ...collection };
    // Clone the specific flashcard being edited, and update the targeted field
    updatedCollection.flashcards[index] = { ...updatedCollection.flashcards[index], [field]: value };
    setCollection(updatedCollection);
  };

  // Save the updated flashcard collection to the database through a put request
  const handleSave = async () => {
    if (!collection) return;

    try {
      await axios.put(`/api/administrator/UpdateFlashcardCollection/${collection.collectionId}`, collection, {
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${localStorage.getItem("jwt")}` },
      });

      alert("Flashcard serien er redigeret!");

    } catch (err) {
      console.error(err);
      console.log("Fejl med at gemme flashcard serien");
    }
  };

  // Upload image function to handle file uploads
  const uploadImage = async (file: File): Promise<string | null> => {
    const formData = new FormData();
    formData.append("file", file);

    try {
      const res = await axios.post("/api/administrator/UploadImage", formData, {
        headers: { "Content-Type": "multipart/form-data", Authorization: `Bearer ${localStorage.getItem("jwt")}` },
      });
      return res.data.imagePath;
    } catch (err) {
      console.error("Image upload failed", err);
      return null;
    }
  };

  return (
    <div className="container">
      <div style={{ position: "relative" }}>
        {" "}
        <img src={BorgertingetIcon} className="Borgertinget-Icon" alt="Borgertinget Icon" />
        <div style={{ position: "absolute", top: "10px", left: "10px" }}>
          {" "}
          
          {/* --- Back button --- */}
          <BackButton match={matchProp} destination="admin" />
        </div>
      </div>
      <div className="top-red-line"></div>

      <h1>Rediger Flashcard serie</h1>

      {/* --- List all Titles --- */}
      <div className="flashcard-titles">
        <h2>Flashcard serier:</h2>
        {titles.map((title, idx) => (
          <button key={idx} onClick={() => fetchCollection(title)} className="flashcard-title-button">
            {title}
          </button>
        ))}
      </div>

      {/* --- Show and edit selected collection --- */}
      {collection && (
        <div className="edit-form">
          <h3 className="flashcard-number">Redigere: {selectedTitle}</h3>

          <div>
            {/* --- Titel --- */}
            <label htmlFor="collectionTitle">Serie Titel:</label>
            <input
              id="collectionTitle"
              type="text"
              value={collection.title}
              onChange={(e) => setCollection((prev) => (prev ? { ...prev, title: e.target.value } : prev))}
              placeholder="Serie titel"
            />
          </div>

          <div>
            {/* --- Description --- */}
            <label htmlFor="collectionDescription">Serie Beskrivelse:</label>
            <textarea
              id="collectionDescription"
              value={collection.description ?? ""}
              onChange={(e) => setCollection((prev) => (prev ? { ...prev, description: e.target.value } : prev))}
              placeholder="Serie beskrivelse"
            />
          </div>

          {/* --- Map through each flashcard in the collection --- */}
          {collection.flashcards.map((fc, index) => (
            <div key={index}>
              <p className="flashcard-number">Flashcard #{index + 1}</p>

              {/* --- Front side --- */}
              {fc.frontContentType === "Text" ? (
                <div>
                  <label htmlFor={`frontText-${index}`}>Front Text:</label>
                  <input
                    id={`frontText-${index}`}
                    type="text"
                    value={fc.frontText ?? ""}
                    onChange={(e) => handleFlashcardChange(index, "frontText", e.target.value)}
                    placeholder="Front Text"
                  />
                </div>
              ) : (
                <div>
                  <label htmlFor={`frontImage-${index}`}>Front billede:</label>
                  <input
                    id={`frontImage-${index}`}
                    type="file"
                    accept="image/*"
                    onChange={async (e) => {
                      const file = e.target.files?.[0];
                      if (!file) return;
                      const path = await uploadImage(file);
                      if (path) {
                        handleFlashcardChange(index, "frontImagePath", path);
                      }
                    }}
                  />
                  {fc.frontImagePath && (
                    <div>
                      <img src={`http://localhost:5218${fc.frontImagePath}`} alt="Front" style={{ width: "150px", marginTop: "10px" }} />
                    </div>
                  )}
                </div>
              )}

              {/* --- Bag side --- */}
              {fc.backContentType === "Text" ? (
                <div>
                  <label htmlFor={`backText-${index}`}>Bag Text:</label>
                  <input
                    id={`backText-${index}`}
                    type="text"
                    value={fc.backText ?? ""}
                    onChange={(e) => handleFlashcardChange(index, "backText", e.target.value)}
                    placeholder="Back Text"
                  />
                </div>
              ) : (
                <div>
                  <label htmlFor={`backImage-${index}`}>Bag billede:</label>
                  <input
                    id={`backImage-${index}`}
                    type="file"
                    accept="image/*"
                    onChange={async (e) => {
                      const file = e.target.files?.[0];
                      if (!file) return;
                      const path = await uploadImage(file);
                      if (path) {
                        handleFlashcardChange(index, "backImagePath", path);
                      }
                    }}
                  />
                  
                  {fc.backImagePath && (
                    <div>
                      {/* app.UseStaticFiles()(backend) == Anything inside wwwroot is publicly accessible via HTTP GET*/}
                      <img src={`http://localhost:5218${fc.backImagePath}`} alt="Back" style={{ width: "150px", marginTop: "10px" }} />
                    </div>
                  )}
                </div>
              )}
            </div>
          ))}

          <button className="save-button" onClick={handleSave}>
            Rediger!
          </button>
        </div>
      )}
    </div>
  );
}
