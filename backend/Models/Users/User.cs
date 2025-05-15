using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace backend.Models
{
    public class User : IdentityUser<int>
    {

        // public string Email { get; set; } = null!;
        // public string UserName { get; set; } = null!;
        // public string PasswordHash { get; set; } = null!;
        // public bool IsVerified { get; set; } = false;
        // public string? VerificationToken { get; set; }
        // public List<string> Roles { get; set; } = new();

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
