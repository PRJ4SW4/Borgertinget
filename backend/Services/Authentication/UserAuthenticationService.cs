using backend.DTOs;
using backend.Models;
using backend.Repositories.Authentication;
using backend.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using MimeKit.Tnef;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt; 
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace backend.Services.Authentication
{
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly IUserAuthenticationRepository _authenticationRepository;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<UserAuthenticationService> _logger;
        private readonly IConfiguration _config;


        public UserAuthenticationService(
            IUserAuthenticationRepository authenticationRepository,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<UserAuthenticationService> logger,
            IConfiguration config)
        {
            _authenticationRepository = authenticationRepository;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _config = config;
        }

        public async Task<User?> FindUserByNameAsync(string username)
        {
            _logger.LogInformation("Attempting to find user by username in service: {Username}", username);
            return await _authenticationRepository.GetUserByNameAsync(username);
        }

        public async Task<IdentityResult> CreateUserAsync(RegisterUserDto dto)
        {
            var user = new User
            {
                UserName = dto.Username,
                Email = dto.Email,
            };

            if (dto.Password == null)
            {
                _logger.LogInformation("Creating user without password: {UserName}", dto.Username);
                return await _userManager.CreateAsync(user);
            }

            return await _userManager.CreateAsync(user, dto.Password);
        }

        public async Task<SignInResult> CheckPasswordSignInAsync(User user, string password, bool lockoutOnFailure)
        {
            return await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return token;
        }

        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
        {
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<IdentityResult> AddToRoleAsync(User user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<IdentityResult> DeleteUserAsync(User user)
        {
            return await _userManager.DeleteAsync(user);        
        }

        public async Task<User?> FindUserByEmailAsync(string email)
        {
            return await _authenticationRepository.GetUserByEmailAsync(email);
        }

        public string? SanitizeReturnUrl(string? clientReturnUrl)
        {
            return (clientReturnUrl ?? "/")
                   .Replace("\n", "")
                   .Replace("\r", "");
        }

        public async Task<User?> GetUserAsync(int userId)
        {
            _logger.LogInformation("Fetching user in service for ID: {UserId}", userId);
            var user = await _authenticationRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return null;
            }
            return new User { Id = user.Id, UserName = user.UserName, Email = user.Email };
        }

        public async Task SignOutAsync()
        {
            _logger.LogInformation("Signing out user in service.");
            await _signInManager.SignOutAsync();
        }

        public async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync()
        {
            _logger.LogInformation("Attempting to get external login info in service.");
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogWarning("Could not retrieve external login information from SignInManager.");
            }
            return info;
        }

        public async Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent, bool bypassTwoFactor)
        {
            _logger.LogInformation("Attempting external login sign-in via service for provider {LoginProvider}.", loginProvider);
            return await _signInManager.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent, bypassTwoFactor);
        }

        public async Task<GoogleLoginResultDto> HandleGoogleLoginCallbackAsync(ExternalLoginInfo info)
        {
            _logger.LogInformation("Handling Google login callback in service for provider {LoginProvider}.", info.LoginProvider);

            var signInResult = await ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true);

            User? appUser;
            if (signInResult.Succeeded)
            {
                appUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (appUser == null)
                {
                    _logger.LogError("User not found with FindByLoginAsync after successful ExternalLoginSignInAsync for {LoginProvider} - {ProviderKey}.", info.LoginProvider, info.ProviderKey);
                    return new GoogleLoginResultDto { Status = GoogleLoginStatus.ErrorUserNotFoundAfterSignIn, ErrorMessage = "Bruger konto problem." };
                }
                _logger.LogInformation("User {UserName} signed in successfully with {LoginProvider}.", appUser.UserName, info.LoginProvider);
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogError("Email claim not found in external principal for provider {LoginProvider}.", info.LoginProvider);
                    return new GoogleLoginResultDto { Status = GoogleLoginStatus.ErrorNoEmailClaim, ErrorMessage = "Email ikke modtaget fra Google." };
                }

                appUser = await _authenticationRepository.GetUserByEmailAsync(email);
                if (appUser == null) // Opret ny lokal bruger
                {
                    var nameFromGoogle = info.Principal.FindFirstValue(ClaimTypes.Name);
                    var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                    var surname = info.Principal.FindFirstValue(ClaimTypes.Surname);

                    string baseUserName;
                    if (!string.IsNullOrEmpty(givenName) && !string.IsNullOrEmpty(surname))
                    {
                        baseUserName = $"{givenName}{surname}";
                    }
                    else if (!string.IsNullOrEmpty(nameFromGoogle))
                    {
                        baseUserName = nameFromGoogle;
                    }
                    else
                    {
                        baseUserName = email.Split('@')[0];
                    }

                    var sanitizedUserName = new string(baseUserName.Where(char.IsLetterOrDigit).ToArray());
                    if (string.IsNullOrWhiteSpace(sanitizedUserName))
                    {
                        sanitizedUserName = $"user{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    }

                    var tempUserName = sanitizedUserName;
                    int count = 1;
                    while (await _authenticationRepository.GetUserByNameAsync(tempUserName) != null)
                    {
                        tempUserName = $"{sanitizedUserName}{count++}";
                    }
                    sanitizedUserName = tempUserName;

                    _logger.LogInformation("Attempting to create new user in service based on external info from {LoginProvider}.", info.LoginProvider);
                    appUser = new User
                    {
                        UserName = sanitizedUserName,
                        Email = email,
                        EmailConfirmed = true, 
                    };

                    var createUserResult = await _userManager.CreateAsync(appUser);
                    if (!createUserResult.Succeeded)
                    {
                        var errorDescriptions = createUserResult.Errors.Select(e => e.Description);
                        _logger.LogError("Failed to create user in service: {Errors}", string.Join(", ", errorDescriptions));
                        return new GoogleLoginResultDto { Status = GoogleLoginStatus.ErrorCreateUserFailed, ErrorMessage = createUserResult.Errors.FirstOrDefault()?.Description ?? "Kunne ikke oprette bruger." };
                    }
                    _logger.LogInformation("Successfully created new user: {UserName}", appUser.UserName);
                }
                else // Bruger fundet via email
                {
                    if (!appUser.EmailConfirmed)
                    {
                        appUser.EmailConfirmed = true;
                        var updateResult = await _userManager.UpdateAsync(appUser);
                        if (!updateResult.Succeeded)
                        {
                            _logger.LogWarning("Could not confirm email for existing user {UserName} during Google Sign In. Errors: {Errors}", appUser.UserName, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                            // Fortsæt alligevel, da det primære formål er login/linking
                        }
                        else
                        {
                            _logger.LogInformation("Email confirmed for existing user {UserName} during Google Sign In.", appUser.UserName);
                        }
                    }
                }

                var addLoginResult = await _userManager.AddLoginAsync(appUser, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                if (!addLoginResult.Succeeded)
                {
                    var errorDescriptions = addLoginResult.Errors.Select(e => e.Description);
                    _logger.LogError("Failed to link external login for user {UserName}: {Errors}", appUser.UserName, string.Join(", ", errorDescriptions));
                    return new GoogleLoginResultDto { Status = GoogleLoginStatus.ErrorLinkLoginFailed, ErrorMessage = "Kunne ikke linke Google konto." };
                }
                _logger.LogInformation("External login linked successfully for user {UserName}.", appUser.UserName);
            }

            // Generer JWT token her i servicen
            var localJwtToken = await GenerateJwtTokenAsync(appUser); 

            return new GoogleLoginResultDto { Status = GoogleLoginStatus.Success, JwtToken = localJwtToken, AppUser = appUser };
        }
        
        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }
            var key = Encoding.UTF8.GetBytes(jwtKey);

            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // Claims specifikke for ASP.NET Core Identity
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                // applikationsspecifikke claims
                new Claim("username", user.UserName ?? string.Empty),
                new Claim("userId", user.Id.ToString()),
            };

            if (userRoles != null)
            {
                foreach (var role in userRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl)
        {
            _logger.LogDebug(
                "Configuring external authentication properties in service for provider {Provider} with redirectUrl {RedirectUrl}",
                provider,
                redirectUrl);
            return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        }
    }
}
