using backend.DTOs;
using backend.Models;
using backend.Repositories.Authentication;
using backend.Utils; 
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens; 
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt; 
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace backend.Services.Authentication
{
    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly IUserAuthenticationRepository _authenticationRepository;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserAuthenticationService> _logger;
        private readonly IConfiguration _config;


        public UserAuthenticationService(
            IUserAuthenticationRepository authenticationRepository,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<UserAuthenticationService> logger,
            IConfiguration config)
        {
            _authenticationRepository = authenticationRepository;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _config = config;
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
                appUser = await _authenticationRepository.FindUserByLoginAsync(info.LoginProvider, info.ProviderKey);
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

                appUser = await _authenticationRepository.FindUserByEmailAsync(email);
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
                    while (await _authenticationRepository.FindUserByNameAsync(tempUserName) != null)
                    {
                        tempUserName = $"{sanitizedUserName}{count++}";
                    }
                    sanitizedUserName = tempUserName;

                    _logger.LogInformation("Attempting to create new user in service based on external info from {LoginProvider}.", info.LoginProvider);
                    appUser = new User
                    {
                        UserName = sanitizedUserName,
                        Email = email,
                        EmailConfirmed = true, // Antag email fra Google er bekræftet
                    };

                    var createUserResult = await _authenticationRepository.CreateUserAsync(appUser);
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
                        var updateResult = await _authenticationRepository.UpdateUserAsync(appUser);
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

                var addLoginResult = await _authenticationRepository.AddLoginAsync(appUser, new UserLoginInfo(info.LoginProvider, info.ProviderKey, info.ProviderDisplayName));
                if (!addLoginResult.Succeeded)
                {
                    var errorDescriptions = addLoginResult.Errors.Select(e => e.Description);
                    _logger.LogError("Failed to link external login for user {UserName}: {Errors}", appUser.UserName, string.Join(", ", errorDescriptions));
                    return new GoogleLoginResultDto { Status = GoogleLoginStatus.ErrorLinkLoginFailed, ErrorMessage = "Kunne ikke linke Google konto." };
                }
                _logger.LogInformation("External login linked successfully for user {UserName}.", appUser.UserName);
            }

            // Generer JWT token her i servicen
            var localJwtToken = await GenerateJwtTokenAsync(appUser); // Antager at appUser ikke er null her

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

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
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
    }
}
