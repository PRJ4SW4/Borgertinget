using System.Collections.Generic;
using backend.Enums;

namespace backend.DTO
{
    /// DTO til at sende resultatet af et gæt tilbage til frontend.
    public class GuessResultDto
    {
        /// Var gættet på den korrekte politiker (samme ID)?
        public bool IsCorrectGuess { get; set; }
        /// Detaljeret feedback per attribut (feltnavn -> feedbacktype).
        /// Nøglerne kan f.eks. være "Køn", "Parti", "Alder", "Region", "Uddannelse".
        /// Frontend bruger disse nøgler til at vise feedback korrekt.
        public Dictionary<string, FeedbackType> Feedback { get; set; } = new Dictionary<string, FeedbackType>();
        /// Detaljer om den politiker, der blev gættet på.
        /// Bruges til at vise info om gættet i frontend.
        /// Genbruger den eksisterende DailyPolticianDto.
        public DailyPoliticianDto? GuessedPolitician { get; set; }
    }
}