
namespace backend.DTOs
{
// denne dto bruges til at hente tweets fra twitter api og vise dem i vores frontend

public class TweetDto
{
    public string Text { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public int Likes { get; set; }
    public int Retweets { get; set; }
    public int Replies { get; set; }
       

}

}