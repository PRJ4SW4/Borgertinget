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
    // --- Del 1 af den sammensatte primærnøgle ---
    // Også fremmednøgle til Politiker tabellen
    // [Key] // Markering som Key her er KUN nødvendig for ældre EF Core versioner, hvis Fluent API ikke bruges.
    [Column("politiker_id")] // Matcher kolonnenavnet i databasen
    public int PolitikerId { get; set; }

    // --- Del 2 af den sammensatte primærnøgle ---
    // [Key] // Markering som Key her er KUN nødvendig for ældre EF Core versioner, hvis Fluent API ikke bruges.
    [Required] // GameMode må ikke være null
    [Column("gamemode")] // Matcher kolonnenavnet
    public GamemodeTypes GameMode { get; set; } // Initialiser for non-nullable string

    // --- Andre kolonner ---
    [Column("lastselecteddate")]
    public DateOnly? LastSelectedDate { get; set; } // Gemmer KUN dato, ikke tidspunkt

    [Column("algovægt")] // Bemærk: Overvej 'AlgoVaegt' som C# navn for konsekvens
    public int? AlgoWeight { get; set; } //* Vægt vil være [days since last selection] i heltal

    // --- Navigation Property ---
    // Dette repræsenterer relationen tilbage til den Politiker, som denne tracking-række tilhører.
    // EF Core bruger dette + PolitikerId til at forstå fremmednøgle-relationen.
    [ForeignKey(nameof(PolitikerId))] // Eksplicit angivelse af fremmednøgle-property
    public virtual FakePolitiker FakePolitiker { get; set; } = null!; // Navigation property til den relaterede politiker
}
