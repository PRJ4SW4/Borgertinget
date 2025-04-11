// Dtos/AnswerCheckRequestDto.cs
using System.ComponentModel.DataAnnotations;

public class AnswerCheckRequestDto
{
    [Required]
    public int QuestionId { get; set; } // ID of the question being answered

    [Required]
    public int SelectedAnswerOptionId { get; set; } // ID of the option the user selected
}

// TODO: Should be moved to a different file
// Dtos/AnswerCheckResponseDto.cs
public class AnswerCheckResponseDto
{
    public bool IsCorrect { get; set; } // Was the selected option the correct one?
    // TODO: Could add more feedback here later if needed
    // public string? Explanation { get; set; }
    // public int CorrectAnswerOptionId { get; set; }
}
