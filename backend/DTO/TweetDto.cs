// Placeres f.eks. i backend.DTOs/TweetDto.cs
using System;

namespace backend.DTOs
{
    public class TweetDto
    {
        // Unikt ID fra Twitter - Godt til 'key' prop i React lister
        public required string TwitterTweetId { get; set; }

        // Selve tweet-indholdet
        public required string Text { get; set; }
        public string? ImageUrl { get; set; } // Kan være null hvis intet billede

        // Engagement-tal
        public int Likes { get; set; }
        public int Retweets { get; set; }
        public int Replies { get; set; }

        // Tidsstempel - Bliver typisk en ISO 8601 streng i JSON
        public DateTime CreatedAt { get; set; }

        // Afsender information (hentes fra din PoliticianTwitterIds tabel)
        public required string AuthorName { get; set; } // F.eks. "Statsministeriet"
        public required string AuthorHandle { get; set; } // F.eks. "Statsmin"
    }
}
