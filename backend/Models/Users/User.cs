using Microsoft.AspNetCore.Identity;

namespace backend.Models
{
    public class User : IdentityUser<int>
    {
        public void OnTweetPosted(object? sender, Tweet tweet)
        {
            var politician = sender as PoliticianTwitterId;
            Console.WriteLine(
                $"[Notification for {UserName}]: {politician?.Name} tweeted: {tweet.Text}"
            );
        }

        public List<Subscription> Subscriptions { get; set; } = new(); // Navigation property for subscriptions
    }
}
