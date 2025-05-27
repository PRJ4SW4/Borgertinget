namespace backend.Models.LearningEnvironment;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Question
{
    [Key]
    public int QuestionId { get; set; }

    [Required]
    public string QuestionText { get; set; } = string.Empty;

    [Required]
    public int PageId { get; set; }

    [ForeignKey("PageId")]
    public virtual Page Page { get; set; } = null!;

    public virtual ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
}
