// Dtos/AnswerCheckRequestDto.cs
using System.ComponentModel.DataAnnotations;

public class AnswerCheckRequestDto
{
    [Required]
    public int QuestionId { get; set; } // ID of the question being answered

    [Required]
    public int SelectedAnswerOptionId { get; set; } // ID of the option the user selected
}

// Dtos/AnswerCheckResponseDto.cs
public class AnswerCheckResponseDto
{
    public bool IsCorrect { get; set; } // Was the selected option the correct one?
    // Optional: You could add more feedback here later if needed
    // public string? Explanation { get; set; }
    // public int CorrectAnswerOptionId { get; set; }
}
