using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

// This interface defines a contract for a calendar service that provides methods to interact with calendar events.
namespace backend.Services.Authentication
{
    public interface IUserAuthenticationService
    {
        Task<User?> GetUserAsync(int userId);
        string? SanitizeReturnUrl(string? clientReturnUrl);

        // User Registration
        Task<IdentityResult> CreateUserAsync(RegisterUserDto dto);
        Task<string> GenerateEmailConfirmationTokenAsync(User user);
        Task<IdentityResult> ConfirmEmailAsync(User user, string token);
        Task<string> GeneratePasswordResetTokenAsync(User user);
        Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);
        Task<IdentityResult> AddToRoleAsync(User user, string role);
        Task<IdentityResult> DeleteUserAsync(User user);
        Task<User?> FindUserByEmailAsync(string email);
        Task<User?> FindUserByNameAsync(string username);

        Task<SignInResult> CheckPasswordSignInAsync(
            User user,
            string password,
            bool lockoutOnFailure
        );

        Task<ExternalLoginInfo?> GetExternalLoginInfoAsync();
        Task<SignInResult> ExternalLoginSignInAsync(
            string loginProvider,
            string providerKey,
            bool isPersistent,
            bool bypassTwoFactor
        );
        Task<GoogleLoginResultDto> HandleGoogleLoginCallbackAsync(ExternalLoginInfo info);
        Task<string> GenerateJwtTokenAsync(User user);

        AuthenticationProperties ConfigureExternalAuthenticationProperties(
            string provider,
            string redirectUrl
        );

        Task SignOutAsync();
    }
}
