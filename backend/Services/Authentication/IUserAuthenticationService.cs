using System.Collections.Generic;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Models;
using Microsoft.AspNetCore.Identity;

// This interface defines a contract for a calendar service that provides methods to interact with calendar events.
namespace backend.Services.Authentication
{
    public interface IUserAuthenticationService
    {
        Task<User?> GetUserAsync(int userId);
        string? SanitizeReturnUrl(string? clientReturnUrl);

        Task<ExternalLoginInfo?> GetExternalLoginInfoAsync();
        Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent, bool bypassTwoFactor);
        Task<GoogleLoginResultDto> HandleGoogleLoginCallbackAsync(ExternalLoginInfo info);
        public Task<string> GenerateJwtTokenAsync(User user); 
    }
}