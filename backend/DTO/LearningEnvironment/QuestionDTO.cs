namespace backend.DTO.LearningEnvironment;

public class QuestionDTO
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public List<AnswerOptionDTO> Options { get; set; } = new List<AnswerOptionDTO>();
}
