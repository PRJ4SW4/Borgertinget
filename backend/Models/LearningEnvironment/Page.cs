namespace backend.Models.LearningEnvironment;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Page
{
    [Key] // Primary Key
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    // I store the Markdown content here.
    public string Content { get; set; } = string.Empty;

    // Foreign key for self-referencing hierarchy
    public int? ParentPageId { get; set; } // Nullable for top-level pages

    // Navigation property for the parent page
    [ForeignKey("ParentPageId")]
    public virtual Page? ParentPage { get; set; }

    // Navigation property for child pages
    public virtual ICollection<Page> ChildPages { get; set; } = new List<Page>();

    // Order field for specific sorting under a parent
    public int DisplayOrder { get; set; } = 0;

    // A list of a pages associated questions
    public virtual ICollection<Question> AssociatedQuestions { get; set; } = new List<Question>();
}
