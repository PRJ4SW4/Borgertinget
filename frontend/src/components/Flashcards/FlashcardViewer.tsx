import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { fetchFlashcardCollectionDetails } from '../../services/ApiService';
import type { FlashcardCollectionDetailDto, FlashcardDto } from '../../types/flashcardTypes';
import './FlashcardViewer.css';

function FlashcardViewer() {
    // Get collectionId from route parameter (defined in FlashcardLayout)
    const { collectionId } = useParams<{ collectionId: string }>();

    // State for the fetched collection data
    const [collectionDetails, setCollectionDetails] = useState<FlashcardCollectionDetailDto | null>(null);
    // State for loading status
    const [isLoading, setIsLoading] = useState<boolean>(true);
    // State for potential errors during fetch
    const [error, setError] = useState<string | null>(null);

    // State for the currently visible flashcard index
    const [currentIndex, setCurrentIndex] = useState<number>(0);
    // State to track if the front (true) or back (false) is visible
    const [isFrontVisible, setIsFrontVisible] = useState<boolean>(true);

    // Effect hook to fetch data when collectionId changes
    useEffect(() => {
        // Validate collectionId before fetching
        if (!collectionId) {
            setError("Ingen samlings-ID fundet i URL.");
            setIsLoading(false);
            setCollectionDetails(null);
            return;
        }

        const loadData = async () => {
            setIsLoading(true);
            setError(null);
            setCollectionDetails(null); // Clear previous data
            setCurrentIndex(0);       // Reset index to first card
            setIsFrontVisible(true);  // Reset to show front side
            try {
                console.log(`Workspaceing details for collection ID: ${collectionId}`); // Debugging
                const data = await fetchFlashcardCollectionDetails(collectionId);
                if (data) {
                    console.log("Fetched Data:", data); // Debugging
                    setCollectionDetails(data);
                    // Handle empty collection case
                    if(data.flashcards.length === 0){
                        setError(`"${data.title}" samlingen indeholder ingen flashcards.`);
                    }
                } else {
                    // API returned null (e.g., 404 Not Found)
                    setError(`Flashcard samling ikke fundet (ID: ${collectionId}).`);
                }
            } catch (err) {
                 // Handle errors during API call
                 setError(err instanceof Error ? err.message : "Ukendt fejl ved indlæsning af flashcards.");
                 console.error("FlashcardViewer fetch error:", err);
            } finally {
                 setIsLoading(false); // Ensure loading stops
            }
        };

        loadData();
    }, [collectionId]); // Re-run this effect if the collectionId in the URL changes

    // --- Interaction Handlers ---

    // Toggle between front and back side
    const handleFlip = () => {
        setIsFrontVisible(prev => !prev);
    };

    // Go to the next card in the collection
    const handleNext = () => {
        // Check if collectionDetails and flashcards exist, and if not already at the last card
        if (collectionDetails && currentIndex < collectionDetails.flashcards.length - 1) {
            setCurrentIndex(prev => prev + 1); // Increment index
            setIsFrontVisible(true);          // Always show front of new card
        }
    };

    // Go to the previous card in the collection
    const handlePrevious = () => {
        // Check if not already at the first card
        if (currentIndex > 0) {
            setCurrentIndex(prev => prev - 1); // Decrement index
            setIsFrontVisible(true);          // Always show front of new card
        }
    };

    // --- Render Logic ---

    // 1. Handle Loading State
    if (isLoading) {
        return <div className="flashcard-viewer-status">Indlæser flashcards...</div>;
    }

    // 2. Handle Error State
    if (error) {
        return <div className="flashcard-viewer-status error">Fejl: {error}</div>;
    }

    // 3. Handle No Data / Empty Collection State (after loading)
    if (!collectionDetails || collectionDetails.flashcards.length === 0) {
         // Error state handles specific messages, or provide a default here
        return <div className="flashcard-viewer-status">Samling ikke fundet eller indeholder ingen flashcards.</div>;
    }

    // 4. Render the viewer if data is loaded successfully
    const currentCard: FlashcardDto = collectionDetails.flashcards[currentIndex]; // Safe access due to checks above
    const totalCards = collectionDetails.flashcards.length;

    // Determine content for front and back sides
    const frontSideData = {
        type: currentCard.frontContentType,
        text: currentCard.frontText,
        path: currentCard.frontImagePath,
        defaultText: "(Spørgsmål mangler)",
        missingImageText: "(Billede til forside mangler)"
    };
    const backSideData = {
        type: currentCard.backContentType,
        text: currentCard.backText,
        path: currentCard.backImagePath,
        defaultText: "(Svar mangler)",
        missingImageText: "(Billede til bagside mangler)"
    };

    // Helper function to render content for a side
    const renderSideContent = (side: typeof frontSideData) => (
        <div className={`flashcard-content ${side.type === 'Image' ? 'content-image' : 'content-text'}`}>
            {side.type === 'Text' && (
                <p>{side.text || side.defaultText}</p>
            )}
            {side.type === 'Image' && side.path && (
                <img src={side.path} />
            )}
            {side.type === 'Image' && !side.path && (
                <p>{side.missingImageText}</p>
            )}
        </div>
    );

    return (
        <div className="flashcard-viewer">
            {/* Display Collection Title */}
            <h2 className="collection-title">{collectionDetails.title}</h2>

            {/* Clickable Card Area */}
            <div
                className="flashcard-container"
                onClick={handleFlip} // Flip card on click
                title="Klik for at vende kortet" // Tooltip
                role="button" // Indicate interactivity
                aria-live="polite" // Announce changes for screen readers
                tabIndex={0} // Make it focusable
                onKeyDown={(e) => { if (e.key === ' ' || e.key === 'Enter') handleFlip(); }} // Allow flip with Space/Enter
            >
                <div className={`flashcard-flipper ${!isFrontVisible ? 'flipped' : ''}`}>
                    <div className="flashcard-face flashcard-front">
                        {renderSideContent(frontSideData)}
                    </div>
                    <div className="flashcard-face flashcard-back">
                        {renderSideContent(backSideData)}
                    </div>
                </div>
            </div>

            {/* Bottom Navigation Bar */}
            <div className="flashcard-navigation">
                <button
                    onClick={handlePrevious}
                    disabled={currentIndex === 0} // Disable if first card
                    className="nav-button prev"
                    aria-label="Forrige kort" // Accessibility label
                >
                    &lt; Forrige
                </button>
                <span className="card-counter" aria-label={`Kort ${currentIndex + 1} af ${totalCards}`}>
                    {currentIndex + 1} / {totalCards}
                </span>
                <button
                    onClick={handleNext}
                    disabled={currentIndex === totalCards - 1} // Disable if last card
                    className="nav-button next"
                    aria-label="Næste kort" // Accessibility label
                >
                    Næste &gt;
                </button>
            </div>
        </div>
    );
}

export default FlashcardViewer;