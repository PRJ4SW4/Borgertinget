// src/components/PageContent.tsx
import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import { fetchPageDetails, checkAnswer } from '../services/ApiService'; // Import checkAnswer
import type { PageDetailDto } from '../types/pageTypes';
import './PageContent.css'; // Ensure CSS is imported

type PageParams = { pageId: string; };
type SelectedAnswersState = Record<number, number | null>;
// Add state type for feedback message per question
type FeedbackState = Record<number, 'correct' | 'incorrect' | 'pending' | null>;

function PageContent() {
  const { pageId } = useParams<PageParams>();
  const [pageDetails, setPageDetails] = useState<PageDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedAnswers, setSelectedAnswers] = useState<SelectedAnswersState>({});
  // --- State for feedback messages ---
  const [feedback, setFeedback] = useState<FeedbackState>({});

  useEffect(() => {
    if (!pageId) { /*...*/ return; }
    const loadPage = async () => {
      setIsLoading(true); setError(null);
      setSelectedAnswers({});
      setFeedback({}); // Reset feedback on new page load
      try { 
        console.log(`Workspaceing details for pageId: ${pageId}`); // Optional: for debugging
        const details = await fetchPageDetails(pageId);
        setPageDetails(details);
      }
      catch (err) { 
        if (err instanceof Error) { setError(err.message); } else { setError("An unknown error occurred while fetching page details.");}
        console.error(`Failed to load page ${pageId}:`, err);
        setPageDetails(null); // Ensure details are cleared on error
      }
      finally { setIsLoading(false); }
    };

    loadPage();
  }, [pageId]);

  // Allow changing answer only if feedback not given yet for this question
  const handleAnswerChange = (questionId: number, answerOptionId: number) => {
     if (!feedback[questionId] || feedback[questionId] === 'pending') {
        setSelectedAnswers(prev => ({ ...prev, [questionId]: answerOptionId }));
     }
  };

  // --- Updated Submit Handler ---
  const handleSubmitAnswer = async (event: React.FormEvent<HTMLFormElement>, questionId: number) => {
    event.preventDefault();
    const selectedOptionId = selectedAnswers[questionId];

    // Don't proceed if no answer or already checking/checked
    if (selectedOptionId === undefined || selectedOptionId === null || feedback[questionId]) {
        if (!selectedOptionId) alert("Vælg venligst et svar.");
        return;
    }

    setFeedback(prev => ({ ...prev, [questionId]: 'pending' })); // Show pending state

    try {
        // Call the API
        const response = await checkAnswer({
            questionId: questionId,
            selectedAnswerOptionId: selectedOptionId
        });

        // Update feedback state based on API response
        setFeedback(prev => ({
            ...prev,
            [questionId]: response.isCorrect ? 'correct' : 'incorrect'
        }));

    } catch (error) {
        console.error("Error checking answer:", error);
        alert("Der opstod en fejl under tjek af svar."); // Show error to user
        setFeedback(prev => ({ ...prev, [questionId]: null })); // Reset pending state on error
    }
  };

  // --- Render Logic ---
  if (isLoading) return <div>Indlæser sideindhold...</div>;
  if (error) return <div style={{ color: 'red' }}>Fejl ved indlæsning af side: {error}</div>;
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
            const isSubmitted = currentFeedback === 'correct' || currentFeedback === 'incorrect';
            const isPending = currentFeedback === 'pending';
            const canSubmit = !(selectedAnswers[question.id] === undefined || selectedAnswers[question.id] === null);

            return (
              <section key={question.id} className={`question-section feedback-${currentFeedback ?? 'none'}`}>
                <h2>Spørgsmål</h2>
                <p className="question-text">{question.questionText}</p>
                <form className="question-form" onSubmit={(e) => handleSubmitAnswer(e, question.id)}>
                  {/* Disable fieldset after successful submission */}
                  <fieldset className="answer-options" disabled={isSubmitted || isPending}>
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
                          disabled={isSubmitted || isPending} // Disable input as well
                        />
                        <label htmlFor={`option-${option.id}`}>
                          {option.optionText}
                        </label>
                      </div>
                    ))}
                  </fieldset>

                  {/* --- Display Feedback Message --- */}
                  <div className="feedback-message">
                    {currentFeedback === 'correct' && <p className="correct">✅ Korrekt!</p>}
                    {currentFeedback === 'incorrect' && <p className="incorrect">❌ Forkert.</p>}
                    {currentFeedback === 'pending' && <p className="pending">Tjekker svar...</p>}
                  </div>
                  {/* --- End Feedback --- */}

                  {/* Disable button if pending, submitted, or no answer selected */}
                   <button
                      type="submit"
                      className="submit-answer-button"
                      disabled={isSubmitted || isPending || !canSubmit}
                    >
                     {isSubmitted ? 'Svar Modtaget' : (isPending ? 'Tjekker...' : 'Indsend Svar')} &gt;&gt;
                   </button>
                </form>
              </section>
            );
          })}
        </div>
      )}
      {/* --- End Question Sections --- */}

      {/* --- Previous/Next Navigation Buttons --- */}
      {/* ... buttons remain the same ... */}
       <div className="page-navigation-buttons">
           {pageDetails.previousSiblingId ? ( <Link to={`/learning/${pageDetails.previousSiblingId}`} className="page-nav-button prev">&lt; Forrige side</Link>) : ( <span className="page-nav-button disabled prev">&lt; Forrige side</span> )}
           {pageDetails.nextSiblingId ? ( <Link to={`/learning/${pageDetails.nextSiblingId}`} className="page-nav-button next">Næste side &gt;</Link>) : ( <span className="page-nav-button disabled next">Næste side &gt;</span> )}
       </div>
    </article>
  );
}

export default PageContent;