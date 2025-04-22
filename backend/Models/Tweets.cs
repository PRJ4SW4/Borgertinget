using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Tweet
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string? ImageUrl { get; set; }
        public int Likes { get; set; }
        public int Retweets { get; set; }
        public int Replies { get; set; }

        public string TwitterTweetId { get; set; } // Tilføj denne! Gør den unik pr. politiker evt.

        public int PoliticianTwitterId { get; set; } // Foreign key to Politician
        public PoliticianTwitterId Politician { get; set; } // Navigation property

        public DateTime CreatedAt { get; set; } // Add this line
    }
}
