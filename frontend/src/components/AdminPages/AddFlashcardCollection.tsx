import { useState } from "react";
import axios from "axios";
import { FlashcardDto, FlashcardCollectionDetailDto } from "../../types/flashcardTypes";
import "./AddFlashcardCollection.css";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";

export default function CreateFlashcardCollection() {
  const [title, setTitle] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [flashcards, setFlashcards] = useState<FlashcardDto[]>([
    {
      flashcardId: 0,
      frontContentType: "Text",
      frontText: "",
      frontImagePath: null,
      backContentType: "Text",
      backText: "",
      backImagePath: null,
    },
  ]);

  const handleAddFlashcard = (imageFront: boolean, imageBack: boolean = false) => {
    const newCard: FlashcardDto = {
      flashcardId: 0,
      frontContentType: imageFront ? "Image" : "Text",
      frontText: imageFront ? null : "",
      frontImagePath: imageFront ? "" : null,
      backContentType: imageBack ? "Image" : "Text",
      backText: imageBack ? null : "",
      backImagePath: imageBack ? "" : null,
    };

    setFlashcards([...flashcards, newCard]);
  };

  const handleSubmit = async () => {
    const dto: FlashcardCollectionDetailDto = {
      collectionId: 0,
      title,
      description,
      flashcards,
    };

    if (!formIsValid()) {
      alert("Felt mangler at blive udfyldt!");
      return;
    }

    try {
      const res = await axios.post("/api/administrator/PostFlashcardCollection", dto, {
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${localStorage.getItem("jwt")}`,
        },
      });

      alert("Flashcard serie er oprettet!!");
      console.log(res.data);

      // Reset the form
      setTitle("");
      setDescription("");
      setFlashcards([
        {
          flashcardId: 0,
          frontContentType: "Text",
          frontText: "",
          frontImagePath: null,
          backContentType: "Text",
          backText: "",
          backImagePath: null,
        },
      ]);
    } catch (err) {
      console.error(err);
      alert("Error: Flashcard serie kunne ikke oprettes");
    }
  };

  const uploadImage = async (file: File): Promise<string | null> => {
    const formData = new FormData();
    formData.append("file", file);

    try {
      const res = await axios.post("/api/administrator/UploadImage", formData, {
        headers: { "Content-Type": "multipart/form-data", Authorization: `Bearer ${localStorage.getItem("jwt")}` },
      });

      return res.data.imagePath; // /uploads/flashcards/filename.png
    } catch (err) {
      console.error("Image upload failed", err);
      return null;
    }
  };

  const formIsValid = () => {
    if (!title.trim()) return false;
    if (!description.trim()) return false;

    // at least one flashcard and every side filled
    // every returns false if either frontOk or backOK is false
    return flashcards.every((fc) => {
      const frontOk = fc.frontContentType === "Text" ? !!fc.frontText?.trim() : !!fc.frontImagePath; // Check if front is empty
      const backOk = fc.backContentType === "Text" ? !!fc.backText?.trim() : !!fc.backImagePath; // Check if back is empty
      return frontOk && backOk;
    });
  };

  return (
    <div className="create-container">
      <div>
        <img src={BorgertingetIcon} className="Borgertinget-Icon"></img>
      </div>

      <div className="top-red-line"></div>
      <h1>Opret ny Flashcard serie</h1>
      <input type="text" value={title} required onChange={(e) => setTitle(e.target.value)} placeholder="Titel" />

      <br />

      <textarea value={description} required onChange={(d) => setDescription(d.target.value)} placeholder="Beskrivelse" />

      <br />

      {flashcards.map((fc, index) => (
        <div key={index}>
          <p className="flashcard-number">
            Flashcard #{index + 1} - Spørgsmål: {fc.frontContentType}, Svar: {fc.backContentType}
          </p>

          {fc.frontContentType === "Text" ? (
            <input
              type="text"
              required
              placeholder="Spørgsmål"
              value={fc.frontText ?? ""}
              onChange={(e) => {
                const updated = [...flashcards];
                updated[index].frontText = e.target.value;
                setFlashcards(updated);
              }}
            />
          ) : (
            <input
              type="file"
              accept="image/*"
              required
              onChange={async (e) => {
                const file = e.target.files?.[0];
                if (!file) return;

                const imagePath = await uploadImage(file);
                if (imagePath) {
                  const updated = [...flashcards];
                  updated[index].frontImagePath = imagePath;
                  setFlashcards(updated);
                }
              }}
            />
          )}

          {fc.backContentType === "Text" ? (
            <input
              type="text"
              placeholder="Svar"
              value={fc.backText ?? ""}
              required
              onChange={(e) => {
                const updated = [...flashcards];
                updated[index].backText = e.target.value;
                setFlashcards(updated);
              }}
            />
          ) : (
            <input
              type="file"
              accept="image/*"
              required
              onChange={async (e) => {
                const file = e.target.files?.[0];
                if (!file) return;

                const imagePath = await uploadImage(file);
                if (imagePath) {
                  const updated = [...flashcards];
                  updated[index].backImagePath = imagePath;
                  setFlashcards(updated);
                }
              }}
            />
          )}
        </div>
      ))}

      <button className="Button" onClick={() => handleAddFlashcard(false, false)}>
        Tilføj Flashcard
      </button>
      <button className="Button" onClick={() => handleAddFlashcard(true, false)}>
        Tilføj Flashcard med billede spørgsmål
      </button>
      <button className="Button" onClick={() => handleAddFlashcard(false, true)}>
        Tilføj Flashcard med billede svar
      </button>

      <br />
      <button className="Button" onClick={handleSubmit}>
        Opret!
      </button>
    </div>
  );
}
