// backend/Models/PolidleGame/DailySelection.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models // Ensure this namespace matches your other Polidle models
{
    [Table("daily_selections")]
    public class DailySelection
    {
        // Part 1 of composite PK
        [Column("selection_date")]
        public DateOnly SelectionDate { get; set; }

        // Part 2 of composite PK
        [Column("gamemode")]
        [Required]
        public GamemodeTypes GameMode { get; set; } // Assuming GamemodeTypes enum exists

        [Column("selected_politiker_id")] // This will be FK to Aktor.Id
        public int SelectedPolitikerID { get; set; }

        [Column("selected_quote_text")]
        public string? SelectedQuoteText { get; set; }

        // Navigation property to Aktor
        [ForeignKey(nameof(SelectedPolitikerID))]
        public virtual Aktor SelectedPolitiker { get; set; } = null!;
    }
}