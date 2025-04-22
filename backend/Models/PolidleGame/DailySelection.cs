// DailySelection.cs
namespace backend.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("daily_selections")]
public class DailySelection
{
    // Del 1 af sammensat PK
    [Column("selection_date")]
    public DateOnly SelectionDate { get; set; }

    // Del 2 af sammensat PK
    [Column("gamemode")]
    [Required]
    public GamemodeTypes GameMode { get; set; }

    // FK til den valgte politiker
    [Column("selected_politiker_id")]
    public int SelectedPolitikerID { get; set; }

    // === NY PROPERTY ===
    // Gemmer teksten for det specifikke citat valgt for Citat-mode på denne dato.
    // Er null for andre gamemodes. Gør den nullable (string?).
    [Column("selected_quote_text")]
    public string? SelectedQuoteText { get; set; }
    // ==================

    // Navigation property tilbage til politikeren (uændret)
    [ForeignKey(nameof(SelectedPolitikerID))]
    public virtual FakePolitiker SelectedPolitiker { get; set; } = null!;
}