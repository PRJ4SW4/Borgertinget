// backend.DTOs/PollDetailsDto.cs
using System;
using System.Collections.Generic;

namespace backend.DTOs
{
    public class PollDetailsDto
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public bool IsActive => !EndedAt.HasValue || EndedAt.Value > DateTime.UtcNow; // hvis den er inden fro ended at, er den aktiv

        // Info om hvem pool er lavet af
        public int PoliticianId { get; set; } // Politikerens DB ID
        public string PoliticianName { get; set; } = string.Empty;
        public string PoliticianHandle { get; set; } = string.Empty;

        // Svarmuligheder
        public List<PollOptionDto> Options { get; set; } = new List<PollOptionDto>();

        public int? CurrentUserVoteOptionId { get; set; } = null; // Hvilken option har brugeren stemt på?
        public int TotalVotes { get; set; }
    }

    public class PollSummaryDto
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public int PoliticianTwitterId { get; set; }
    }
}
