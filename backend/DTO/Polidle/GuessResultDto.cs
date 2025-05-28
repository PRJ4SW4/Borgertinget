using backend.Enums;

namespace backend.DTO
{
    public class GuessResultDto
    {
        public bool IsCorrectGuess { get; set; }

        public Dictionary<string, FeedbackType> Feedback { get; set; } =
            new Dictionary<string, FeedbackType>();

        public DailyPoliticianDto? GuessedPolitician { get; set; }
    }
}
