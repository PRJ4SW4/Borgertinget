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
        public bool IsActive => !EndedAt.HasValue || EndedAt.Value > DateTime.UtcNow; // Beregnet property

        // Info om ophavsmand
        public int PoliticianId { get; set; } // Politikerens DB ID
        public string PoliticianName { get; set; } = string.Empty;
        public string PoliticianHandle { get; set; } = string.Empty;

        // Svarmuligheder
        public List<PollOptionDto> Options { get; set; } = new List<PollOptionDto>();

        // Info til den aktuelle bruger (kan tilføjes)
        public int? CurrentUserVoteOptionId { get; set; } = null; // Hvilken option har brugeren stemt på? (null hvis ikke stemt)
        public int TotalVotes { get; set; } // Samlet antal stemmer på pollen
    }
}
