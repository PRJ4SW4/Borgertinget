// src/components/PollCard.tsx
import React, { useState } from 'react';
import { PollDetailsDto, /*PollOptionDto*/ } from '../../types/tweetTypes'; // Ret sti om nødvendigt
import './PollCard.css'; // Importer CSS

interface PollCardProps {
  poll: PollDetailsDto;
  // Funktion der kaldes, når brugeren klikker på en stemmeknap
  onVoteSubmit: (pollId: number, optionId: number) => Promise<void>;
}

const PollCard: React.FC<PollCardProps> = ({ poll, onVoteSubmit }) => {
  // State til at vise loading/disable knapper MENS der stemmes
  const [isVoting, setIsVoting] = useState<boolean>(false);

  const handleVoteClick = async (optionId: number) => {
    if (isVoting || !poll.isActive /*| poll.currentUserVoteOptionId !== null*/) // fix, jeg har outcommenteret poll.currentUserVoteOptionId !== null, da det er den der tjekker om brugeren har stemt, så har brugeren ikke milgihed for 
    // at ændre stemme, hvis han er træt af hvad han stemte :-)
       { 
      
      return;
    }
    setIsVoting(true); // Start loading state
    try {
      // Kald funktionen fra FeedPage, som kalder API'et
      await onVoteSubmit(poll.id, optionId);
      // UI opdatering sker via SignalR eller den lokale opdatering i FeedPage
    } catch (error) {
      // Fejl håndteres og vises i FeedPage
      console.error("Vote submission failed (handled in FeedPage)", error);
    } finally {
      setIsVoting(false); // Stop loading state (uanset succes/fejl)
    }
  };

  const formatDateTime = (dateString?: string | null) => {
    if (!dateString) return '';
    try {
        return new Date(dateString).toLocaleString('da-DK', { dateStyle: 'short', timeStyle: 'short' });
    } catch { return "Ugyldig dato"; }
  };

  return (
    <div className={`poll-card ${!poll.isActive ? 'poll-inactive' : ''}`}>
      {/* Tweet Header*/}
      <div className="poll-header">
        <div className="author-info">
          <strong className="author-name">{poll.politicianName}</strong>
          <span className="author-handle">@{poll.politicianHandle}</span>
        </div>
        <small className="timestamp">{formatDateTime(poll.createdAt)}</small>
      </div>

      <p className="poll-question">{poll.question}</p>

      <div className="poll-options">
        {poll.options.map((option) => {
          const percentage = poll.totalVotes === 0 ? 0 : Math.round((option.votes / poll.totalVotes) * 100);
          const isUsersVote = poll.currentUserVoteOptionId === option.id;

          return (
            <div
              key={option.id}
              className={`poll-option ${isUsersVote ? 'voted-option' : ''}`}
            >
              {/* Vis stemmeknap hvis aktiv OG bruger IKKE har stemt */}
              {poll.isActive /*&& poll.currentUserVoteOptionId === null && */ &&
                  <button
                      className="vote-button"
                      onClick={() => handleVoteClick(option.id)}
                     /* disabled={isVoting} */
                     disabled={isVoting || poll.currentUserVoteOptionId === option.id}
                  >
                      Stem
                  </button>
              }
               {/* Vis flueben hvis bruger har stemt på denne */}
               {isUsersVote && <span className="vote-checkmark">✓</span>}

              <div className="option-text">{option.optionText}</div>
              {/* Vis resultater hvis pollen er inaktiv ELLER brugeren har stemt */}
              {(!poll.isActive || poll.currentUserVoteOptionId !== null) && (
                  <>
                    <div className="vote-bar-container">
                       <div
                           className="vote-bar"
                           style={{ width: `${percentage}%` }} 
                       ></div>
                    </div>
                    <div className="vote-percentage">{percentage}%</div>
                    <div className="vote-count">({option.votes})</div>
                  </>
              )}
            </div>
          );
        })}
      </div>

      <div className="poll-footer">
        <span>Total stemmer: {poll.totalVotes}</span>
        <span>
          {poll.isActive
            ? (poll.endedAt ? `Lukker: ${formatDateTime(poll.endedAt)}` : 'Åben')
            : 'Afsluttet'}
        </span>
      </div>
    </div>
  );
};

export default PollCard;