namespace backend.DTO.LearningEnvironment;

using System.ComponentModel.DataAnnotations;

public class QuestionCreateOrUpdateDTO
{
    public int Id { get; set; } // 0 for new, >0 for existing to update

    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string QuestionText { get; set; } = string.Empty;

    public List<AnswerOptionCreateOrUpdateDTO> Options { get; set; } =
        new List<AnswerOptionCreateOrUpdateDTO>();
}
