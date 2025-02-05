using System.ComponentModel.DataAnnotations;

namespace UserAuthentication.DTOs
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "Brugernavn er påkrævet")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email er påkrævet")]
        [EmailAddress(ErrorMessage = "Email er ikke gyldig")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password er påkrævet")]
        [RegularExpression(
            pattern: @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$",
            ErrorMessage = "Kodeordet skal have mindst 8 tegn, mindst én stor og én lille bogstav, samt mindst ét tal")]
        public string Password { get; set; }
    }
}
