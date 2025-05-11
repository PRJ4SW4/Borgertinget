using System.ComponentModel.DataAnnotations;
using backend.Models; // For GamemodeTypes enum

namespace backend.DTO
{
    public class GuessRequestDto
    {
        [Required]
        public int GuessedPoliticianId { get; set; }

        [Required]
        public GamemodeTypes GameMode { get; set; }

        // Evt. Tilføj UserId senere, hvis gæt skal knyttes til en bruger
        // public string? UserId { get; set; }
    }
}
