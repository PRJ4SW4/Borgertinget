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

        // 1. Foreign Key Property:
        //    Holder værdien af Id'et fra den relaterede Aktor.
        //    Konventionen er [PrincipalEntityName]Id.
        public int AktorId { get; set; }

        // 2. Navigation Property:
        //    Giver adgang til den relaterede Aktor-entitet.
        //    ForeignKey-attributten fortæller EF Core, at AktorId-property'et
        //    er foreign key for denne navigation property.
        //    'virtual' muliggør lazy loading.
        [ForeignKey(nameof(AktorId))]
        public virtual Aktor Politician { get; set; } = null!;
    }
}
