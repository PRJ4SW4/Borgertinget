using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Politicians;

namespace backend.Models
{
    public class PoliticianQuote
    {
        [Key]
        public int QuoteId { get; set; } // Primærnøgle for selve citatet

        [Required] // Gør selve citatteksten påkrævet
        public string QuoteText { get; set; } = string.Empty;

        // --- Relation til Aktor ---
        public int AktorId { get; set; }

        [ForeignKey(nameof(AktorId))]
        public virtual Aktor Politician { get; set; } = null!;
    }
}
