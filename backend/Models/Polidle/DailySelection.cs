using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Enums; // Sørg for at enum er tilgængelig

namespace backend.Models
{
    [Table("daily_selections")] // Angiver tabelnavn
    public class DailySelection
    {
        // --- Del 1 af Sammensat Primærnøgle ---
        // Konfigureres via Fluent API
        [Column("selection_date")]
        public DateOnly SelectionDate { get; set; } // Kun dato

        // --- Del 2 af Sammensat Primærnøgle ---
        // Konfigureres via Fluent API
        [Column("gamemode")]
        [Required]
        public GamemodeTypes GameMode { get; set; } // Fra enum

        // --- Fremmednøgle til Aktor ---
        [Column("selected_politiker_id")] // Database kolonnenavn
        public int SelectedPolitikerID { get; set; } // Refererer til Aktor.Id

        // --- Specifik data for valget ---
        // Nullable, da det kun er relevant for Citat-mode
        [Column("selected_quote_text")]
        public string? SelectedQuoteText { get; set; }

        // --- Navigation Property til Aktor ---
        [ForeignKey(nameof(SelectedPolitikerID))] // Linker til FK property ovenfor
        public virtual Aktor SelectedPolitiker { get; set; } = null!; // Giver adgang til Aktor-objektet
    }
}
