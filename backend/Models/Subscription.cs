using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
public class Subscription
{
    public int Id { get; set; }
    public int UserId { get; set; } // The user who is subscribing
    public int PoliticianTwitterId { get; set; } // The politician the user is subscribing to

    // Navigation properties
    public User User { get; set; }
    public PoliticianTwitterId Politician { get; set; }
}

}