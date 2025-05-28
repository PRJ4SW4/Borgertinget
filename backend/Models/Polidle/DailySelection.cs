using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Enums;
using backend.Models.Politicians;

namespace backend.Models
{
    [Table("daily_selections")]
    public class DailySelection
    {
        // --- Del 1 af Sammensat Primærnøgle ---
        [Column("selection_date")]
        public DateOnly SelectionDate { get; set; }

        // --- Del 2 af Sammensat Primærnøgle ---
        [Column("gamemode")]
        [Required]
        public GamemodeTypes GameMode { get; set; }

        // --- Fremmednøgle til Aktor ---
        [Column("selected_politiker_id")]
        public int SelectedPolitikerID { get; set; }

        [Column("selected_quote_text")]
        public string? SelectedQuoteText { get; set; }

        // --- Navigation Property til Aktor ---
        [ForeignKey(nameof(SelectedPolitikerID))]
        public virtual Aktor? SelectedPolitiker { get; set; } = null!;
    }
}
