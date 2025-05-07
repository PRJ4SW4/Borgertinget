namespace backend.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum GamemodeTypes
{
    Klassisk = 0, // Default
    Citat = 1,
    Foto = 2,
}

public class PolidleGamemodeTracker
{
    // Part 1 of composite PK & FK to Aktor
    [Column("politiker_id")]
    public int PolitikerId { get; set; }

    // Part 2 of composite PK
    [Column("gamemode")]
    [Required]
    public GamemodeTypes GameMode { get; set; }

    [Column("lastselecteddate")]
    public DateOnly? LastSelectedDate { get; set; }

    [Column("algovægt")]
    public int? AlgoWeight { get; set; }

    // Navigation Property to Aktor
    [ForeignKey(nameof(PolitikerId))]
    public virtual Aktor Aktor { get; set; } = null!;
}