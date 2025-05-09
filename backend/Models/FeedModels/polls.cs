// backend.Models/Poll.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// i polls models.cs er der på nuværende tidspunkt 3 forskellige
// klasser: Poll, PollOption og UserVote.
// Poll repræsenterer en afstemning, PollOption repræsenterer de forskellige svarmuligheder
// til afstemningen, og UserVote repræsenterer en brugers stemme på en bestemt mulighed.
// Grunden til at jeg har valgt at opdele dem i separate klasser er for at holde koden ren og overskuelig.
// på nuværende tidspunkt er de herinde i en models fil, da det ellers vil blive bloated, med models
namespace backend.Models
{
    public class Poll
    {
        public int Id { get; set; } // Primærnøgle for Poll

        [Required]
        [MaxLength(500)]
        public string Question { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }

        [Required]
        public int PoliticianId { get; set; } // Foreign Key til PoliticianTwitterId tabellen

        [Required]
        public string? PoliticianTwitterId { get; set; } // foreign key til PoliticianTwitterId tabellen
        public virtual PoliticianTwitterId Politician { get; set; } = null!; // relation til PoliticianTwitterId, dette for at lave en poll der tilhøre en politiker

        // Relation til Svar-muligheder, da vi skal have en liste af pollotpions, som jo er de svar muligheder,
        public virtual List<PollOption> Options { get; set; } = new List<PollOption>();

        // Relation til Afgivne Stemmmer på de forskellige option muligheder
        public virtual List<UserVote> UserVotes { get; set; } = new List<UserVote>();
    }

    public class PollOption
    {
        public int Id { get; set; } // Primærnøgle for PollOption

        [Required]
        [MaxLength(200)]
        public string OptionText { get; set; } = string.Empty;

        public int Votes { get; set; } = 0; // Antal stemmer

        // Relation til Poll
        [Required]
        public int PollId { get; set; } // Foreign Key til Poll tabellen
        public virtual Poll Poll { get; set; } = null!; // Navigation Property

        // Relation til Afgivne Stemmer (for denne option)
        public virtual List<UserVote> UserVotes { get; set; } = new List<UserVote>();
    }

    public class UserVote
    {
        public int Id { get; set; } // Primærnøgle for UserVote

        // Relation til Bruger
        [Required]
        public int UserId { get; set; } // Foreign Key til User tabellen
        public virtual User User { get; set; } = null!; // Navigation Property

        // Relation til Poll
        [Required]
        public int PollId { get; set; } // Foreign Key til Poll tabellen
        public virtual Poll Poll { get; set; } = null!; // Navigation Property

        // Relation til Valgt Svar
        [Required]
        public int ChosenOptionId { get; set; } // Foreign Key til PollOption tabellen
        public virtual PollOption ChosenOption { get; set; } = null!; // Navigation Property
    }

    public class PollSummaryDto
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string PoliticianTwitterId { get; set; } = string.Empty;
    }
}
