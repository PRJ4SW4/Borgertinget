namespace backend.Models.Flashcards;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class FlashcardCollection
{
    [Key]
    public int CollectionId { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public virtual ICollection<Flashcard> Flashcards { get; set; } = new List<Flashcard>();
}
