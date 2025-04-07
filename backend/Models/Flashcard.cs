// Models/Flashcard.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum FlashcardContentType
{
    Text = 0, // Default
    Image = 1,
}

public class Flashcard
{
    [Key]
    public int FlashcardId { get; set; }

    [Required]
    public int CollectionId { get; set; } // FK

    [ForeignKey("CollectionId")]
    public virtual FlashcardCollection FlashcardCollection { get; set; } = null!;

    public int DisplayOrder { get; set; } = 0;

    // Front Side
    [Required]
    public FlashcardContentType FrontContentType { get; set; } = FlashcardContentType.Text;
    public string? FrontText { get; set; } // Null if Image

    [StringLength(500)] // Example max path length
    public string? FrontImagePath { get; set; } // Null if Text, relative path like /uploads/flashcards/abc.png

    // Back Side
    [Required]
    public FlashcardContentType BackContentType { get; set; } = FlashcardContentType.Text;
    public string? BackText { get; set; }

    [StringLength(500)]
    public string? BackImagePath { get; set; }
}
