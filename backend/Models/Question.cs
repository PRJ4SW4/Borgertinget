// Models/Question.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Question
{
    [Key]
    public int QuestionId { get; set; }

    [Required]
    public string QuestionText { get; set; } = string.Empty;

    // Foreign Key to Page
    [Required]
    public int PageId { get; set; }

    [ForeignKey("PageId")]
    public virtual Page Page { get; set; } = null!; // Navigation property back to Page

    // Navigation property to AnswerOptions
    public virtual ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
}
