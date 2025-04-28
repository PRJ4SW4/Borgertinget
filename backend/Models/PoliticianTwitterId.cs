using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class PoliticianTwitterId
    {
        public int Id { get; set; }
        public string TwitterUserId { get; set; }
        public string Name { get; set; }
        public string TwitterHandle { get; set; }

        public void PostTweet(Tweet tweet)
        {
            tweet.PoliticianTwitterId = this.Id;
            tweet.CreatedAt = DateTime.Now;
            Tweets.Add(tweet); //  in-memory tracking

            TweetPosted?.Invoke(this, tweet); // raise event to notify all subscribers
        }

        public event EventHandler<Tweet>? TweetPosted;
        public List<Tweet> Tweets { get; set; } = new();
        public List<Subscription> Subscriptions { get; set; } = new();
    }
}
