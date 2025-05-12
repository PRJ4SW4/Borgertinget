namespace backend.DTO.LearningEnvironment;

using System.ComponentModel.DataAnnotations;

public class AnswerOptionCreateOrUpdateDTO
{
    public int Id { get; set; } // 0 for new, >0 for existing to update

    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string OptionText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public int DisplayOrder { get; set; } = 0;
}
