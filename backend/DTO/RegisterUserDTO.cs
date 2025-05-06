using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "Brugernavn er påkrævet")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Email er påkrævet")]
        [EmailAddress(ErrorMessage = "Email er ikke gyldig")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password er påkrævet")]
        public required string Password { get; set; }
    }
}
