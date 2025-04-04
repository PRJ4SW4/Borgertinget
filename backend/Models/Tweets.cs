
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public class Tweet
{
    public int Id { get; set; } 
    public string Text { get; set; } 
    public string? ImageUrl { get; set; } 
    public int Likes { get; set; } 
    public int Retweets { get; set; } 
    public int Replies { get; set; } 
    public string UserId { get; set; } 
    public DateTime CreatedAt { get; set; } 
}
