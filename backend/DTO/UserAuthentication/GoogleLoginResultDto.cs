using backend.Models;

namespace backend.DTOs
{
    public enum GoogleLoginStatus
    {
        Success,
        ErrorExternalProvider,
        ErrorNoLoginInfo,
        ErrorNoEmailClaim,
        ErrorCreateUserFailed,
        ErrorLinkLoginFailed,
        ErrorUserNotFoundAfterSignIn
    }

    public class GoogleLoginResultDto
    {
        public GoogleLoginStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public string? JwtToken { get; set; } 
        public User? AppUser { get; set; } 
    }
}
