using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
public class PoliticianTwitterId
{
    public int Id { get; set; }
    public string TwitterUserId { get; set; }  // The unique Twitter ID of the politician
    public string Name { get; set; }
    public string TwitterHandle { get; set; }

    public List<Tweet> Tweets { get; set; } = new();  // List of tweets associated with this politician
    public List<Subscription> Subscriptions { get; set; } = new();  // List of users subscribing to this politician
}
}