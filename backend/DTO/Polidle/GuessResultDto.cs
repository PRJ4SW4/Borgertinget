using System.Collections.Generic;
using backend.Enums;

namespace backend.DTO
{
    public class GuessResultDto
    {
        public bool IsCorrectGuess { get; set; }

        /// Detaljeret feedback per attribut (feltnavn -> feedbacktype).
        /// Nøglerne er  "Køn", "Parti", "Alder", "Region", "Uddannelse".
        /// Frontend bruger disse nøgler til at vise feedback korrekt.
        public Dictionary<string, FeedbackType> Feedback { get; set; } =
            new Dictionary<string, FeedbackType>();

        /// Detaljer om den politiker, der blev gættet på.
        /// Bruges til at vise info om gættet i frontend.
        /// Genbruger den eksisterende DailyPolticianDto.
        public DailyPoliticianDto? GuessedPolitician { get; set; }
    }
}
