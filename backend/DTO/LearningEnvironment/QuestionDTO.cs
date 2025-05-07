// /backend/DTO/LearningEnvironment/QuestionDTO.cs
namespace backend.DTO.LearningEnvironment;

public class QuestionDTO
{
    public int Id { get; set; } // QuestionId
    public string QuestionText { get; set; } = string.Empty;
    public List<AnswerOptionDTO> Options { get; set; } = new List<AnswerOptionDTO>();
}
