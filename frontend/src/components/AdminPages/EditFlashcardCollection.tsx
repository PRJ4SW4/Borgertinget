import { useEffect, useState } from "react";
import axios from "axios";
import {FlashcardDto, FlashcardCollectionDetailDto, FlashcardContentType} from "../../types/flashcardTypes";


export default function EditFlashcardCollection() {
    const [titles, setTitles] = useState<string[]>([]);
    const [selectedTitle, setSelectedTitle] = useState<string>("");
    const [collection, setCollection] = useState<FlashcardCollectionDetailDto | null>(null);

    // Load all titles
    useEffect(() => {
        const fetchTitles = async () => {
            try {
                const res = await axios.get<string[]>("/api/administrator/GetAllFlashcardCollectionTitles");
                setTitles(res.data);
            } catch (err) {
                console.error(err);
            }
        };

        fetchTitles();
    }, []);

    // Fetch collection when a title is clicked
    const fetchCollection = async (title:string) => {
        try {
            const res = await axios.get<FlashcardCollectionDetailDto>(`/api/administrator/GetFlashcardCollectionByTitle?title=${encodeURIComponent(title)}`);
            setCollection(res.data);
            setSelectedTitle(title);
        } catch(err) {
            console.error(err);
        }
    };

    const handleFlashcardChange = (index: number, field: keyof FlashcardDto, value: string) => {
        if (!collection) return;

        const updatedCollection = { ...collection };
        updatedCollection.flashcards[index] = { ...updatedCollection.flashcards[index], [field]: value};
        setCollection(updatedCollection);
    };

    // Save the updated flashcard collection to the database through af put request
    const handleSave = async () => {
        if (!collection) return;

        try {
            await axios.put(`/api/administrator/UpdateFlashcardCollection/${collection.collectionId}`, collection, {
                headers: { "Content-Type": "application/json"},
            });
            alert("Flashcard serie gemt!");
        } catch (err) {
            console.error(err);
            console.log("Error saving collection");
        }

    };


    // Upload image function to handle file uploads
    const uploadImage = async (file: File): Promise<string | null> => {
        const formData = new FormData();
        formData.append("file", file);

        try {
            const res = await axios.post("/api/administrator/UploadImage", formData, {
              headers: { "Content-Type": "multipart/form-data" },
            });
            return res.data.imagePath;
          } catch (err) {
            console.error("Image upload failed", err);
            return null;
          }
        };

    return (
        <div>
          <h1>Rediger Flashcard serie</h1>
      
          {/* List all Titles */}
          <div>
            <h2>Flashcard serier:</h2>
            {titles.map((title, idx) => (
              <button key={idx} onClick={() => fetchCollection(title)}>
                {title}
              </button>
            ))}
          </div>
      
          {/* Show and edit selected collection */}
          {collection && (
        <div>
              <h3>Redigere: {selectedTitle}</h3>
          
              <div>
                <label>Serie Titel:</label>
                <input
                  type="text"
                  value={collection.title}
                  onChange={(e) =>
                    setCollection(prev => prev ? { ...prev, title: e.target.value } : prev)
                  }
                  placeholder="Serie titel"
                />
              </div>
      
              <div>
                <label>Serie Beskrivelse:</label>
                <textarea
                  value={collection.description ?? ""}
                  onChange={(e) =>
                    setCollection(prev => prev ? { ...prev, description: e.target.value } : prev)
                  }
                  placeholder="Serie beskrivelse"
                />
              </div>
      
              {/* Map through each flashcard in the collection */}
              {collection.flashcards.map((fc, index) => (
                <div key={index}>
                  <p>Flashcard #{index + 1}</p>
      
                  {/* Front side */}
                  {fc.frontContentType === "Text" ? (
                    <div>
                      <label>Front Text:</label>
                      <input
                        type="text"
                        value={fc.frontText ?? ""}
                        onChange={(e) => handleFlashcardChange(index, "frontText", e.target.value)}
                        placeholder="Front Text"
                      />
                    </div>
                  ) : (
                    <div>
                      <label>Front billede:</label>
                      <input
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
                          <img
                            src={`http://localhost:5218${fc.frontImagePath}`}
                            alt="Front"
                            style={{ width: "150px", marginTop: "10px" }}
                          />
                        </div>
                      )}
                    </div>
                  )}
      
                  {/* Bag side */}
                  {fc.backContentType === "Text" ? (
                    <div>
                      <label>Bag Text:</label>
                      <input
                        type="text"
                        value={fc.backText ?? ""}
                        onChange={(e) => handleFlashcardChange(index, "backText", e.target.value)}
                        placeholder="Back Text"
                      />
                    </div>
                  ) : (
                    <div>
                      <label>Bag billede:</label>
                      <input
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
                          <img
                            src={`http://localhost:5218${fc.backImagePath}`}
                            alt="Back"
                            style={{ width: "150px", marginTop: "10px" }}
                          />
                        </div>
                      )}
                    </div>
                  )}
                </div>
              ))}
          
              <button onClick={handleSave}>Save Changes</button>
            </div>
          )}
        </div>
      );
}