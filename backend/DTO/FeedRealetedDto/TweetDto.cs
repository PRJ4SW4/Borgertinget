using System;

namespace backend.DTOs
{
    public class TweetDto
    {
        
        public required string TwitterTweetId { get; set; }

        
        public required string Text { get; set; }
        public string? ImageUrl { get; set; } 

        
        public int Likes { get; set; }
        public int Retweets { get; set; }
        public int Replies { get; set; }

        
        public DateTime CreatedAt { get; set; }

        
        public required string AuthorName { get; set; }  
        public required string AuthorHandle { get; set; } 
    }
}