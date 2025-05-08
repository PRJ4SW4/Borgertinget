using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Tweet
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Likes { get; set; }
        public int Retweets { get; set; }
        public int Replies { get; set; }

        public string TwitterTweetId { get; set; } = string.Empty;

        public int PoliticianTwitterId { get; set; }
        public PoliticianTwitterId Politician { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
