import React, { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import ReactMarkdown from "react-markdown";
import { fetchPageDetails, checkAnswer } from "../../services/ApiService";
import type { PageDetailDto } from "../../types/pageTypes";
import "./PageContent.css";

type PageParams = { pageId: string };
type SelectedAnswersState = Record<number, number | null>;
// Add state type for feedback message per question
type FeedbackState = Record<number, "correct" | "incorrect" | "pending" | null>;
// Add state for timeout IDs
type TimeoutIdsState = Record<number, NodeJS.Timeout | null>;

function PageContent() {
  const { pageId } = useParams<PageParams>();
  const [pageDetails, setPageDetails] = useState<PageDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedAnswers, setSelectedAnswers] = useState<SelectedAnswersState>({});
  // --- State for feedback messages ---
  const [feedback, setFeedback] = useState<FeedbackState>({});
  // --- State for timeout IDs ---
  const [timeoutIds, setTimeoutIds] = useState<TimeoutIdsState>({});

  useEffect(() => {
    // Cleanup function to clear all timeouts
    const cleanupAllTimeouts = () => {
      Object.values(timeoutIds).forEach((timeoutId) => {
        if (timeoutId) {
          clearTimeout(timeoutId);
        }
      });
      setTimeoutIds({}); // Reset the state tracking timeouts
    };

    if (!pageId) {
      cleanupAllTimeouts(); // Clear timeouts if navigating away to a non-pageId route
      // Reset other states if necessary
      setPageDetails(null);
      setSelectedAnswers({});
      setFeedback({});
      setIsLoading(false);
      setError(null);
      return;
    }

    // Clear timeouts from any previous page or interaction before loading new page
    cleanupAllTimeouts();

    const loadPage = async () => {
      setIsLoading(true);
      setError(null);
      setSelectedAnswers({}); // Reset answers for the new page
      setFeedback({}); // Reset feedback for the new page
      // timeoutIds is already reset by cleanupAllTimeouts above

      try {
        console.log(`Fetching details for pageId: ${pageId}`); // For debugging
        const details = await fetchPageDetails(pageId);
        setPageDetails(details);
      } catch (err) {
        if (err instanceof Error) {
          setError(err.message);
        } else {
          setError("An unknown error occurred while fetching page details.");
        }
        console.error("Failed to load page %s:", pageId, err);
        setPageDetails(null); // Ensure details are cleared on error
      } finally {
        setIsLoading(false);
      }
    };

    loadPage();

    // Return the cleanup function to be called on component unmount or before effect re-runs for a new pageId
    return cleanupAllTimeouts;
  }, [pageId, timeoutIds]); // pageId is the key dependency. timeoutIds is managed internally.

  // Allow changing answer only if feedback not given yet for this question
  const handleAnswerChange = (questionId: number, answerOptionId: number) => {
    // Allow change if feedback is undefined (initial), null (after incorrect timeout),
    // or pending (though UI might disable interaction during pending).
    if (feedback[questionId] === undefined || feedback[questionId] === null || feedback[questionId] === "pending") {
      setSelectedAnswers((prev) => ({ ...prev, [questionId]: answerOptionId }));
    }
  };

  // --- Updated Submit Handler ---
  const handleSubmitAnswer = async (event: React.FormEvent<HTMLFormElement>, questionId: number) => {
    event.preventDefault();
    const selectedOptionId = selectedAnswers[questionId];

    // Don't proceed if no answer, or already correctly answered, or currently pending
    if (selectedOptionId === undefined || selectedOptionId === null || feedback[questionId] === "correct" || feedback[questionId] === "pending") {
      if (!selectedOptionId && feedback[questionId] !== "correct" && feedback[questionId] !== "pending") {
        alert("Vælg venligst et svar.");
      }
      return;
    }

    // Clear any existing timeout for this specific question before submitting again
    if (timeoutIds[questionId]) {
      clearTimeout(timeoutIds[questionId]!);
      setTimeoutIds((prev) => ({ ...prev, [questionId]: null }));
    }

    setFeedback((prev) => ({ ...prev, [questionId]: "pending" })); // Show pending state

    try {
      const response = await checkAnswer({
        questionId: questionId,
        selectedAnswerOptionId: selectedOptionId,
      });

      if (response.isCorrect) {
        setFeedback((prev) => ({
          ...prev,
          [questionId]: "correct",
        }));
      } else {
        setFeedback((prev) => ({
          ...prev,
          [questionId]: "incorrect",
        }));
        // Set a timeout to allow re-answering after 5 seconds
        const newTimeoutId = setTimeout(() => {
          setFeedback((prev) => ({ ...prev, [questionId]: null })); // Reset feedback
          setSelectedAnswers((prev) => ({ ...prev, [questionId]: null })); // Clear selection
          setTimeoutIds((prev) => ({ ...prev, [questionId]: null })); // Clear the stored timeout ID
        }, 5000);
        setTimeoutIds((prev) => ({ ...prev, [questionId]: newTimeoutId }));
      }
    } catch (error) {
      console.error("Error checking answer:", error);
      alert("Der opstod en fejl under tjek af svar."); // Show error to user
      setFeedback((prev) => ({ ...prev, [questionId]: null })); // Reset pending state on error
      if (timeoutIds[questionId]) {
        // Ensure any new timeout is cleared on error too
        clearTimeout(timeoutIds[questionId]!);
        setTimeoutIds((prev) => ({ ...prev, [questionId]: null }));
      }
    }
  };

  // --- Render Logic ---
  if (isLoading) return <div>Indlæser sideindhold...</div>;
  if (error) return <div style={{ color: "red" }}>Fejl ved indlæsning af side: {error}</div>;
  if (!pageDetails) return <div>Siden blev ikke fundet eller data mangler.</div>;

  return (
    <article className="page-content-article">
      <h1>{pageDetails.title}</h1>
      <div className="markdown-body">
        <ReactMarkdown>{pageDetails.content}</ReactMarkdown>
      </div>

      {/* --- Question Sections --- */}
      {pageDetails.associatedQuestions && pageDetails.associatedQuestions.length > 0 && (
        <div className="all-questions-container">
          {pageDetails.associatedQuestions.map((question) => {
            const currentFeedback = feedback[question.id];
            const isCorrectlyAnswered = currentFeedback === "correct";
            const isPending = currentFeedback === "pending";
            const isIncorrectAndWait = currentFeedback === "incorrect"; // User has answered incorrectly and is in the 5s wait period

            const isDisabled = isCorrectlyAnswered || isPending || isIncorrectAndWait;
            const canSubmit = !(selectedAnswers[question.id] === undefined || selectedAnswers[question.id] === null);

            const getButtonText = () => {
              if (isCorrectlyAnswered) return "Svar Modtaget";
              if (isIncorrectAndWait) return "Forkert, prøv igen om lidt...";
              if (isPending) return "Tjekker...";
              return "Indsend Svar";
            };

            return (
              <section key={question.id} className={`question-section feedback-${currentFeedback ?? "none"}`}>
                <h2>Spørgsmål</h2>
                <p className="question-text">{question.questionText}</p>
                <form className="question-form" onSubmit={(e) => handleSubmitAnswer(e, question.id)}>
                  {/* Disable fieldset if correctly answered, pending, or incorrect (waiting for timeout) */}
                  <fieldset className="answer-options" disabled={isDisabled}>
                    <legend className="sr-only">Svar muligheder for spørgsmål {question.id}</legend>
                    {question.options.map((option) => (
                      <div key={option.id} className="answer-option">
                        <input
                          type="radio"
                          id={`option-${option.id}`}
                          name={`question-${question.id}`}
                          value={option.id}
                          checked={selectedAnswers[question.id] === option.id}
                          onChange={() => handleAnswerChange(question.id, option.id)}
                          required
                          disabled={isDisabled} // Disable input as well
                        />
                        <label htmlFor={`option-${option.id}`}>{option.optionText}</label>
                      </div>
                    ))}
                  </fieldset>

                  {/* --- Display Feedback Message --- */}
                  <div className="feedback-message">
                    {currentFeedback === "correct" && <p className="correct">✅ Korrekt!</p>}
                    {currentFeedback === "incorrect" && <p className="incorrect">❌ Forkert.</p>}
                    {currentFeedback === "pending" && <p className="pending">Tjekker svar...</p>}
                  </div>
                  {/* --- End Feedback --- */}

                  {/* Disable button if processing, correctly answered, incorrect (waiting), or no answer selected */}
                  <button type="submit" className="submit-answer-button" disabled={isDisabled || !canSubmit}>
                    {getButtonText()} &gt;&gt;
                  </button>
                </form>
              </section>
            );
          })}
        </div>
      )}
      {/* --- End Question Sections --- */}

      {/* --- Previous/Next Navigation Buttons --- */}
      <div className="page-navigation-buttons">
        {pageDetails.previousSiblingId ? (
          <Link to={`/learning/${pageDetails.previousSiblingId}`} className="page-nav-button prev">
            &lt; Forrige side
          </Link>
        ) : (
          <span className="page-nav-button disabled prev">&lt; Forrige side</span>
        )}
        {pageDetails.nextSiblingId ? (
          <Link to={`/learning/${pageDetails.nextSiblingId}`} className="page-nav-button next">
            Næste side &gt;
          </Link>
        ) : (
          <span className="page-nav-button disabled next">Næste side &gt;</span>
        )}
      </div>
    </article>
  );
}

export default PageContent;
