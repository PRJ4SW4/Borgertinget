namespace backend.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PoliticianTwitterId { get; set; }

        public User User { get; set; } = null!;
        public PoliticianTwitterId Politician { get; set; } = null!;
    }
}
