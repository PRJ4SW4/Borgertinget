// /backend/DTO/LearningEnvironment/AnswerCheckRequestDTO.cs
namespace backend.DTO.LearningEnvironment;

using System.ComponentModel.DataAnnotations;

public class AnswerCheckRequestDTO
{
    [Required]
    public int QuestionId { get; set; } // ID of the question being answered

    [Required]
    public int SelectedAnswerOptionId { get; set; } // ID of the option the user selected
}
