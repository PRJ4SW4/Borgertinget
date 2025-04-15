// /backend/DTO/LearningEnvironment/AnswerCheckResponseDTO.cs
namespace backend.DTO.LearningEnvironment;

public class AnswerCheckResponseDTO
{
    public bool IsCorrect { get; set; } // Was the selected option the correct one?

    // TODO: Could add more feedback here later
    // public string? Explanation { get; set; }
    // public int CorrectAnswerOptionId { get; set; }
}
