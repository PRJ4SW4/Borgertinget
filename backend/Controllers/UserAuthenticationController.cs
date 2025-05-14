using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IConfiguration config,
            EmailService emailService,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<UsersController> logger
        )
        {
            _config = config;
            _emailService = emailService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }

        // POST: api/users
        [HttpPost("register")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterUserDto dto)
        {
            var user = new User { UserName = dto.Username, Email = dto.Email };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, "User");

                if (!roleResult.Succeeded)
                {
                    var errors = roleResult.Errors.Select(e => e.Description);
                    _logger.LogError(
                        $"Failed to assign role 'User' to {user.UserName}: {string.Join(", ", errors)}"
                    );

                    // Optionally, delete the user if assigning the role failed
                    await _userManager.DeleteAsync(user);

                    return StatusCode(
                        500,
                        new
                        {
                            message = "Brugeren blev oprettet, men der opstod en fejl ved tildeling af rollen.",
                            errors,
                        }
                    );
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var verificationLink =
                    $"http://localhost:5173/verify?userId={user.Id}&token={encodedToken}";

                var subject = "Bekræft din e-mailadresse";
                var message =
                    $@"
                    <p>Tak fordi du oprettede en konto.</p>
                    <p>Klik venligst på linket nedenfor for at bekræfte din e-mailadresse:</p>
                    <p><a href='{verificationLink}'>Bekræft min e-mail</a></p>
                    <p>Hvis du ikke kan klikke på linket, kopier og indsæt følgende URL i din browser:</p>
                    <p>{verificationLink}</p>";

                try
                {
                    await _emailService.SendEmailAsync(user.Email, subject, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Fejl ved afsendelse af bekræftelsesmail: {ex.Message}");
                    return StatusCode(
                        500,
                        new
                        {
                            message = "Fejl ved afsendelse af bekræftelsesmail. Prøv venligst igen senere.",
                        }
                    );
                }
                return Ok(
                    new
                    {
                        message = "Registrering succesfuld! Tjek din email for at bekræfte din konto.",
                    }
                );
            }
            else
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogError($"Brugerregistrering fejlede: {string.Join(", ", errors)}");
                return BadRequest(new { errors });
            }
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyEmail(
            [FromQuery] int userId,
            [FromQuery] string token
        )
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Token mangler.");
                return BadRequest("Token mangler.");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogError($"Bruger med ID {userId} blev ikke fundet.");
                return BadRequest("Ugyldigt bruger ID.");
            }

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
                if (result.Succeeded)
                {
                    return Ok(
                        new { message = "Din emailadresse er bekræftet. Du kan nu logge ind." }
                    );
                }
                else
                {
                    Console.WriteLine(
                        $"Email verification failed for user {userId}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                    );
                    return BadRequest("Ugyldigt eller udløbet verifikationslink");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fejl ved afkodning af token");
                return BadRequest(new { message = "Ugyldigt token format" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            string loginInput = dto.EmailOrUsername.ToLower();
            User? user;

            // Find bruger ud fra E-mail eller brugernavn
            if (loginInput.Contains('@'))
            {
                user = await _userManager.FindByEmailAsync(loginInput);
            }
            else
            {
                user = await _userManager.FindByNameAsync(loginInput);
            }

            if (user == null)
                return BadRequest(new { error = "Bruger findes ikke" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

            if (result.Succeeded)
            {
                var token = await GenerateJwtToken(user);
                return Ok(new { token });
            }
            else if (result.IsNotAllowed)
            {
                return BadRequest(
                    new
                    {
                        error = "Din emailadresse er ikke blevet bekræftet. Tjek din indbakke for at bekræfte.",
                    }
                );
            }
            else
            {
                return BadRequest(new { error = "Forkert adgangskode." });
            }
        }

        [AllowAnonymous]
        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle([FromQuery] string? clientReturnUrl = null)
        {
            var sanitizedClientReturnUrl = (clientReturnUrl ?? "/")
                .Replace("\n", "")
                .Replace("\r", "");
            _logger.LogInformation(
                "Start Google login process. Ønsket frontend returnUrl for efterfølgende redirect: {ClientReturnUrl}",
                sanitizedClientReturnUrl
            );

            // Den URL, som SignInManager gemmer i AuthenticationProperties, for at vide hvor OnTicketReceived skal sende os hen.
            // Denne URL (vores HandleGoogleCallback) vil så modtage den oprindelige clientReturnUrl som et query parameter.
            var propertiesRedirectUri = Url.Action(
                action: nameof(HandleGoogleCallback),
                controller: "Users",
                values: new { returnUrl = clientReturnUrl },
                protocol: Request.Scheme
            );

            if (string.IsNullOrEmpty(propertiesRedirectUri))
            {
                _logger.LogError(
                    "Kunne ikke generere URL til HandleGoogleCallback via Url.Action."
                );
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Intern fejl: Kunne ikke starte Google login."
                );
            }

            _logger.LogDebug(
                "Intern redirect URI for ConfigureExternalAuthenticationProperties (til HandleGoogleCallback): {PropertiesRedirectUri}",
                propertiesRedirectUri
            );

            var authenticationProperties = _signInManager.ConfigureExternalAuthenticationProperties(
                GoogleDefaults.AuthenticationScheme,
                propertiesRedirectUri // Denne URL peger på vores HandleGoogleCallback med den oprindelige clientReturnUrl
            );

            return Challenge(authenticationProperties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("HandleGoogleCallback")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleGoogleCallback(
            [FromQuery] string? returnUrl = null,
            [FromQuery] string? remoteError = null
        )
        {
            _logger.LogInformation("Modtaget callback fra Google."); //

            if (!string.IsNullOrEmpty(remoteError))
            {
                var sanitizedRemoteError = remoteError.Replace("\n", "").Replace("\r", "");
                _logger.LogError($"Fejl fra ekstern udbyder: {sanitizedRemoteError}"); //
                return Redirect(
                    $"http://localhost:5173/login?error={HttpUtility.UrlEncode(remoteError)}"
                );
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Kunne ikke hente ekstern login information."); //
                return Redirect(
                    $"http://localhost:5173/login?error={HttpUtility.UrlEncode("Fejl ved eksternt login.")}"
                );
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true
            );

            User appUser;
            if (signInResult.Succeeded)
            {
                appUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (appUser == null)
                {
                    _logger.LogError(
                        $"Bruger ikke fundet med FindByLoginAsync efter succesfuld ExternalLoginSignInAsync for {info.LoginProvider} - {info.ProviderKey}."
                    ); //
                    return Redirect(
                        $"http://localhost:5173/login?error={HttpUtility.UrlEncode("Bruger konto problem.")}"
                    );
                }
                _logger.LogInformation(
                    $"Bruger {appUser.UserName} logget ind med {info.LoginProvider}."
                ); //
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogError("Email claim ikke fundet i eksternt principal."); //
                    return Redirect(
                        $"http://localhost:5173/login?error={HttpUtility.UrlEncode("Email ikke modtaget fra Google.")}"
                    );
                }

                appUser = await _userManager.FindByEmailAsync(email);
                if (appUser == null) // Opret ny lokal bruger
                {
                    var nameFromGoogle = info.Principal.FindFirstValue(ClaimTypes.Name);
                    var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                    var surname = info.Principal.FindFirstValue(ClaimTypes.Surname);

                    string baseUserName;
                    if (!string.IsNullOrEmpty(givenName) && !string.IsNullOrEmpty(surname))
                    {
                        baseUserName = $"{givenName}{surname}"; // F.eks. "ReneSchumacher"
                    }
                    else if (!string.IsNullOrEmpty(nameFromGoogle))
                    {
                        baseUserName = nameFromGoogle;
                    }
                    else
                    {
                        baseUserName = email.Split('@')[0]; // Fallback til email prefix
                    }

                    // Fjern ugyldige tegn (alt undtagen bogstaver og tal)
                    var sanitizedUserName = new string(
                        baseUserName.Where(char.IsLetterOrDigit).ToArray()
                    );

                    // Sørg for at det ikke er tomt efter sanering
                    if (string.IsNullOrWhiteSpace(sanitizedUserName))
                    {
                        sanitizedUserName = $"user{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                    }

                    // Tjek om brugernavnet allerede eksisterer, og tilføj evt. et tal for at gøre det unikt
                    var tempUserName = sanitizedUserName;
                    int count = 1;
                    while (await _userManager.FindByNameAsync(tempUserName) != null)
                    {
                        tempUserName = $"{sanitizedUserName}{count++}";
                    }
                    sanitizedUserName = tempUserName;

                    _logger.LogInformation(
                        "Forsøger at oprette ny bruger baseret på eksterne oplysninger fra Google."
                    );
                    appUser = new User
                    {
                        UserName = sanitizedUserName,
                        Email = email,
                        EmailConfirmed = true,
                    };
                    var createUserResult = await _userManager.CreateAsync(appUser);
                    if (!createUserResult.Succeeded)
                    {
                        _logger.LogError(
                            $"Fejl ved oprettelse af bruger (email redacted): {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}"
                        );
                        string errorDetail =
                            createUserResult.Errors.FirstOrDefault()?.Description
                            ?? "Kunne ikke oprette bruger.";
                        return Redirect(
                            $"http://localhost:5173/login?error={HttpUtility.UrlEncode(errorDetail)}"
                        );
                    }
                }
                else
                {
                    if (!appUser.EmailConfirmed)
                    {
                        appUser.EmailConfirmed = true;
                        await _userManager.UpdateAsync(appUser);
                        _logger.LogInformation(
                            $"Email bekræftet for eksisterende bruger: {appUser.UserName}"
                        ); //
                    }
                }
                var addLoginResult = await _userManager.AddLoginAsync(appUser, info);
                if (!addLoginResult.Succeeded)
                {
                    _logger.LogError(
                        $"Fejl ved at linke eksternt login: {string.Join(", ", addLoginResult.Errors.Select(e => e.Description))}"
                    ); //
                    return Redirect(
                        $"http://localhost:5173/login?error={HttpUtility.UrlEncode("Kunne ikke linke Google konto.")}"
                    );
                }
                _logger.LogInformation("Eksternt login linket for en bruger."); //
            }

            // Ryd den midlertidige eksterne cookie
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var localJwtToken = await GenerateJwtToken(appUser);
            _logger.LogInformation("JWT genereret for en bruger.");

            await _signInManager.SignOutAsync();

            string frontendBaseUrl = _config["FrontendBaseUrl"] ?? "http://localhost:5173";
            string loginSuccessPathOnFrontend = "/login-success";

            var queryParams = new Dictionary<string, string?> { { "token", localJwtToken } };

            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/")) // Simpel validering
            {
                queryParams.Add("originalReturnUrl", returnUrl);
            }
            else if (!string.IsNullOrEmpty(returnUrl))
            {
                var sanitizedReturnUrl = returnUrl.Replace("\n", "").Replace("\r", "");
                _logger.LogWarning(
                    "Ignorerer ugyldig returnUrl ('{OriginalReturnUrl}') modtaget i HandleGoogleCallback for redirect til LoginSuccessPage.",
                    sanitizedReturnUrl
                );
            }

            string urlForLoginSuccessPage = QueryHelpers.AddQueryString(
                $"{frontendBaseUrl}{loginSuccessPathOnFrontend}",
                queryParams
            );

            _logger.LogInformation(
                "Redirecter til frontend's LoginSuccessPage: {FinalUrl}",
                urlForLoginSuccessPage
            );
            return Redirect(urlForLoginSuccessPage);
        }

        private async Task<string> GenerateJwtToken(User user)
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
