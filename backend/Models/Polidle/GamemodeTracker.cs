using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Enums;
using backend.Models.Politicians;

namespace backend.Models
{
    public class GamemodeTracker
    {
        // --- Del 1 af den sammensatte primærnøgle & Fremmednøgle ---
        // Denne property vil blive konfigureret som en del af den sammensatte PK via Fluent API
        [Column("politiker_id")] // Database kolonnenavn
        public int PolitikerId { get; set; } // Refererer til Aktor.Id

        // --- Del 2 af den sammensatte primærnøgle ---
        // Denne property vil blive konfigureret som en del af den sammensatte PK via Fluent API
        [Required]
        [Column("gamemode")]
        public GamemodeTypes GameMode { get; set; } // Bruger enum'en

        [Column("lastselecteddate")]
        public DateOnly? LastSelectedDate { get; set; } // .NET 6+ type for kun dato

        [Column("algovægt")]
        public int? AlgoWeight { get; set; } // Overvej C# navnet AlgoWeight

        [ForeignKey(nameof(PolitikerId))]
        public virtual Aktor Politician { get; set; } = null!;
    }
}
