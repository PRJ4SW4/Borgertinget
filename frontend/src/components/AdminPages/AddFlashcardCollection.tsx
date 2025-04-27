import { useState } from "react";
import axios from "axios";
import {FlashcardDto, FlashcardCollectionDetailDto, FlashcardContentType} from "../../types/flashcardTypes";
import './AddFlashcardCollection.css'
import BorgertingetIcon from "../../images/BorgertingetIcon.png"

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

    const handleAddFlashcard = (asImageFront : boolean, asImageBack: boolean = false) => {
        const newCard: FlashcardDto = {
            flashcardId: 0,
            frontContentType: asImageFront ? "Image" : "Text",
            frontText: asImageFront ? null : "",
            frontImagePath: asImageFront ? "" : null,
            backContentType: asImageBack ? "Image" : "Text",
            backText: asImageBack ? null : "",
            backImagePath: asImageBack ? "" : null,
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

        try {
            const res = await axios.post("/api/administrator/PostFlashcardCollection", dto, {
                headers: {"Content-Type": "application/json" },
            });

            alert(res.data);
            
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

        } catch(err) {
            console.error(err);
            alert("Error: Flashcard collection could not be created")
        };
    }

    const uploadImage = async (file: File): Promise<string | null> => {
        const formData = new FormData();
        formData.append("file", file);

        try {
            const res = await axios.post("/api/administrator/UploadImage", formData, {
              headers: { "Content-Type": "multipart/form-data" },
            });
        
            return res.data.imagePath; // /uploads/flashcards/filename.png
          } catch (err) {
            console.error("Image upload failed", err);
            return null;
          }
    };


    return(
        <div className="create-container">

        <div><img src={BorgertingetIcon} className='Borgertinget-Icon'></img></div>

        <div className='top-red-line'></div>
            <h1>Opret ny Flashcard serie</h1>
            <input
                type="text"
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="Titel"
            />

            <br/> 

            <textarea
                value={description}
                onChange={(d) => setDescription(d.target.value)}
                placeholder="Beskrivelse"
            />

            <br/> 

            {flashcards.map((fc, index) => (
                <div key={index}>
                    <p className="flashcard-number">Flashcard #{index + 1} - Spørgsmål: {fc.frontContentType}, Svar: {fc.backContentType}</p>

                    {fc.frontContentType === "Text" ? (
                        <input
                            type="text"
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

                <button
                    className="Button"
                    onClick={ () => handleAddFlashcard(false, false)}>Tilføj Flashcard</button>
                <button
                    className="Button"
                    onClick={ () => handleAddFlashcard(true, false)}>Tilføj Flashcard med billede spørgsmål</button>
                <button
                    className="Button" 
                    onClick={ () => handleAddFlashcard(false, true)}>Tilføj Flashcard med billede svar</button>
            
            <br/>
            <button 
                className="Button"
                onClick={handleSubmit}>Opret!</button>
        </div>
    );
}