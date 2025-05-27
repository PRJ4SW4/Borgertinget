import { useState } from "react";
import axios from "axios";
import { useLocation } from "react-router-dom";
import {
  FlashcardDto,
  FlashcardCollectionDetailDto,
} from "../../types/flashcardTypes";
import "./AddFlashcardCollection.css";
import BorgertingetIcon from "../../images/BorgertingetIcon.png";
import BackButton from "../Button/backbutton"; 


// Allows an admin to create a new flashcard collection.
export default function CreateFlashcardCollection() {
  const [title, setTitle] = useState<string>("");
  const location = useLocation();
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
  const matchProp = { path: location.pathname };

  // Adds a new empty flashcard to the collection input list
  const handleAddFlashcard = (
    imageFront: boolean,
    imageBack: boolean = false
  ) => {
    // Define the new flashcard and define the flashcard based on imageFront and imageBack
    const newCard: FlashcardDto = {
      flashcardId: 0,
      frontContentType: imageFront ? "Image" : "Text",
      frontText: imageFront ? null : "",
      frontImagePath: imageFront ? "" : null,
      backContentType: imageBack ? "Image" : "Text",
      backText: imageBack ? null : "",
      backImagePath: imageBack ? "" : null,
    };

    // create a new array that includes all the items in an existing list plus one (or more) new items
    setFlashcards([...flashcards, newCard]);
  };

  // Converts current state into a DTO-compatible payload and sends it to the backend.
  // The backend expects a title, description, and a list of flashcards with text/image info.
  const handleSubmit = async () => {
    // Save the useState variables defined before into a dto
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
      const res = await axios.post(
        "/api/administrator/PostFlashcardCollection",
        dto,
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("jwt")}`,
          },
        }
      );

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

  // Uploads an image file to the backend and sets the image path for display and saving
  // The returned relative path is stored in the appropriate field (front or back)
  const uploadImage = async (file: File): Promise<string | null> => {
    const formData = new FormData();
    formData.append("file", file);

    try {
      const res = await axios.post("/api/administrator/UploadImage", formData, {
        headers: {
          "Content-Type": "multipart/form-data",
          Authorization: `Bearer ${localStorage.getItem("jwt")}`,
        },
      });

      return res.data.imagePath; // /uploads/flashcards/filename.png
    } catch (err) {
      console.error("Image upload failed", err);
      return null;
    }
  };

  // Helper function to check if the inputfields are filled
  const formIsValid = () => {
    if (!title.trim()) return false;
    if (!description.trim()) return false;

    // at least one flashcard and every side filled
    // every returns false if either frontOk or backOK is false
    return flashcards.every((fc) => {
      const frontOk =
        fc.frontContentType === "Text"
          ? !!fc.frontText?.trim()
          : !!fc.frontImagePath; // Check if front is empty
      const backOk =
        fc.backContentType === "Text"
          ? !!fc.backText?.trim()
          : !!fc.backImagePath; // Check if back is empty
      return frontOk && backOk;
    });
  };

  return (
    <div className="create-container">
      <div style={{ position: "relative" }}>
        {" "}
        {/* Added for positioning context */}
        <img
          src={BorgertingetIcon}
          className="Borgertinget-Icon"
          alt="Borgertinget Icon"
        />
        <div style={{ position: "absolute", top: "10px", left: "10px" }}>
          {" "}
          {/* Adjust top/left as needed */}
          <BackButton match={matchProp} destination="admin" />
        </div>
      </div>

      <div className="top-red-line"></div>
      <h1>Opret ny Flashcard serie</h1>
      {/* ---- Titel ----- */}
      <input
        type="text"
        value={title}
        required
        onChange={(e) => setTitle(e.target.value)}
        placeholder="Titel"
      />

      <br />

      {/* ---- Description ----- */}
      <textarea
        value={description}
        required
        onChange={(d) => setDescription(d.target.value)}
        placeholder="Beskrivelse"
      />

      <br />

      {/* ---- Flashcards ----- */}
      {flashcards.map((fc, index) => (
        <div key={index}>
          <p className="flashcard-number">
            Flashcard #{index + 1} - Spørgsmål: {fc.frontContentType}, Svar:{" "}
            {fc.backContentType}
          </p>

          {/* ---- Front/question ----- */}
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

          {/* ---- Back/Answer ----- */}
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

      {/* ---- Add new flashcard buttons ----- */}
      <button
        className="Button"
        onClick={() => handleAddFlashcard(false, false)}
      >
        Tilføj Flashcard
      </button>
      <button
        className="Button"
        onClick={() => handleAddFlashcard(true, false)}
      >
        Tilføj Flashcard med billede spørgsmål
      </button>
      <button
        className="Button"
        onClick={() => handleAddFlashcard(false, true)}
      >
        Tilføj Flashcard med billede svar
      </button>

      <br />

      {/* ---- Submit button ----- */}
      <button className="Button" onClick={handleSubmit}>
        Opret!
      </button>
    </div>
  );
}
