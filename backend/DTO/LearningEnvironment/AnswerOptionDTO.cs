namespace backend.DTO.LearningEnvironment;

public class AnswerOptionDTO
{
    public int Id { get; set; } // AnswerOptionId
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int DisplayOrder { get; set; }
}
