// Models/AnswerOption.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class AnswerOption
{
    [Key]
    public int AnswerOptionId { get; set; }

    [Required]
    public string OptionText { get; set; } = string.Empty;

    [Required]
    public bool IsCorrect { get; set; }

    public int DisplayOrder { get; set; } = 0; // Optional ordering

    // Foreign Key to Question
    [Required]
    public int QuestionId { get; set; }

    [ForeignKey("QuestionId")]
    public virtual Question Question { get; set; } = null!; // Navigation property back to Question
}
