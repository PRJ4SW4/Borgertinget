using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class PoliticianTwitterId
    {
        public int Id { get; set; }
        public string TwitterUserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TwitterHandle { get; set; } = string.Empty;

        public int? AktorId { get; set; }

        public List<Tweet> Tweets { get; set; } = new();
        public List<Subscription> Subscriptions { get; set; } = new();
        public virtual Aktor? Aktor { get; set; }

        public virtual List<Poll> Polls { get; set; } = new List<Poll>();
    }
}
