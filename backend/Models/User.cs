namespace backend.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool IsVerified { get; set; } = false;
        public string? VerificationToken { get; set; }
        public List<string> Roles { get; set; } = new();

        public List<Subscription> Subscriptions { get; set; } = new(); // Navigation property for subscriptions

    }
}
