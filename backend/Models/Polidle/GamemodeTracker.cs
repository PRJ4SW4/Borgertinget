using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Enums;
using backend.Models.Politicians;

namespace backend.Models
{
    public class GamemodeTracker
    {
        // --- Del 1 af den sammensatte primærnøgle & Fremmednøgle ---
        [Column("politiker_id")] // Database kolonnenavn
        public int PolitikerId { get; set; }

        // --- Del 2 af den sammensatte primærnøgle ---
        [Required]
        [Column("gamemode")]
        public GamemodeTypes GameMode { get; set; }

        [Column("lastselecteddate")]
        public DateOnly? LastSelectedDate { get; set; }

        [Column("algovægt")]
        public int? AlgoWeight { get; set; }

        [ForeignKey(nameof(PolitikerId))]
        public virtual Aktor Politician { get; set; } = null!;
    }
}
