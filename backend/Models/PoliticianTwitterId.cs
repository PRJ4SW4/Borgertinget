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

    public List<Tweet> Tweets { get; set; } = new();    
    public List<Subscription> Subscriptions { get; set; } = new();  
    
    public virtual List<Poll> Polls { get; set; } = new List<Poll>(); 


}
}