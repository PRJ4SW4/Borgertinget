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
using backend.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using backend.Services.Authentication;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase // Corrected class name based on common practice and constructor
    {
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<UsersController> _logger; // Corrected logger type to match class name
        private readonly IUserAuthenticationService _userAuthenticationService;


        public UsersController(
            IConfiguration config,
            EmailService emailService,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<UsersController> logger,
            IUserAuthenticationService userAuthenticationService 
        )
        {
            _config = config;
            _emailService = emailService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _userAuthenticationService = userAuthenticationService;
        }

        // POST: api/users
        [HttpPost("register")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterUserDto dto)
        {

            _logger.LogInformation("Brugerregistrering igangsat for {Email}", dto.Email);

            var result = await _userAuthenticationService.CreateUserAsync(dto);

            if (result.Succeeded)
            {
                var user = await _userAuthenticationService.FindUserByEmailAsync(dto.Email);

                var roleResult = await _userAuthenticationService.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                {
                    var errors = roleResult.Errors.Select(e => e.Description);
                    _logger.LogError(
                        $"Fejl ved tildeling af rolle 'User' til {LogSanitizer.Sanitize(user.UserName)}: {string.Join(", ", errors)}"
                    );
                    await _userAuthenticationService.DeleteUserAsync(user);

                    return StatusCode(500, new { message = "Brugeren blev oprettet, men der opstod en fejl ved tildeling af rolle.", errors });
                }

                var token = await _userAuthenticationService.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var verificationLink = $"http://localhost:5173/verify?userId={user.Id}&token={encodedToken}";
                var subject = "Bekræft din e-mailadresse";
                var message = $@"<p>Tak fordi du oprettede en konto.</p>
                           <p>Klik venligst på linket nedenfor for at bekræfte din e-mailadresse:</p>
                           <p><a href='{verificationLink}'>Bekræft min e-mail</a></p>";
                try
                {
                    await _emailService.SendEmailAsync(dto.Email, subject, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Fejl ved afsendelse af bekræftelsesmail: {ex.Message}");
                    return StatusCode(500, new { message = "Fejl ved afsendelse af mail. Prøv venligst igen senere." });
                }

                return Ok(new { message = "Registrering succesfuld! Tjek din email for at bekræfte din konto." });
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
                user = await _userAuthenticationService.FindUserByEmailAsync(loginInput);
            }
            else
            {
                user = await _userAuthenticationService.FindUserByNameAsync(loginInput);
            }

            if (user == null)
                return BadRequest(new { error = "Bruger findes ikke" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

            if (result.Succeeded)
            {
                var token = await _userAuthenticationService.GenerateJwtTokenAsync(user);
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

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return BadRequest(new { error = "Bruger findes ikke." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var resetLink =
                $"http://localhost:5173/reset-password?userId={user.Id}&token={encodedToken}";

            var subject = "Nulstil din adgangskode";
            var message =
                $@"
                <p>Du anmodede om at nulstille din adgangskode.</p>
                <p>Klik venligst på linket nedenfor for at nulstille din adgangskode:</p>
                <p><a href='{resetLink}'>Nulstil adgangskode</a></p>";

            try
            {
                await _emailService.SendEmailAsync(user.Email, subject, message);
                return Ok(
                    new
                    {
                        message = "En mail med et link til at nulstille din adgangskode er blevet sendt.",
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Mail kunne ikke sendes: {ex.Message}");
                return StatusCode(
                    500,
                    new
                    {
                        message = "Fejl ved afsendelse af nulstillingsmail. Prøv venligst igen senere.",
                    }
                );
            }
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordDto dto,
            [FromQuery] int userId,
            [FromQuery] string token
        )
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return BadRequest(new { error = "Bruger findes ikke." });
            }

            if (dto.NewPassword != dto.ConfirmPassword)
            {
                return BadRequest(new { error = "Adgangskoderne skal matche." });
            }

            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ResetPasswordAsync(
                    user,
                    decodedToken,
                    dto.NewPassword
                );
                if (result.Succeeded)
                {
                    return Ok(new { message = "Adgangskoden er blevet ændret." });
                }
                else
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(new { errors });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fejl ved afkodning af token");
                return BadRequest(new { message = "Ugyldigt token format" });
            }
        }

        [AllowAnonymous]
        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle([FromQuery] string? clientReturnUrl = null)
        {
            var sanitizedClientReturnUrl = _userAuthenticationService.SanitizeReturnUrl(clientReturnUrl);

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
            [FromQuery] string? remoteError = null)
        {
            _logger.LogInformation("Modtaget callback fra Google i controller.");

            if (!string.IsNullOrEmpty(remoteError))
            {
                var sanitizedRemoteError = _userAuthenticationService.SanitizeReturnUrl(remoteError); 
                _logger.LogError("Fejl fra ekstern udbyder (Google): {RemoteError}", sanitizedRemoteError);
                return Redirect($"http://localhost:5173/login?error={HttpUtility.UrlEncode(remoteError)}");
            }

            var info = await _userAuthenticationService.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Kunne ikke hente ekstern login information i controlleren.");
                return Redirect($"http://localhost:5173/login?error={HttpUtility.UrlEncode("Fejl ved eksternt login.")}");
            }

            // Kald service til at håndtere resten af logikken
            var loginResult = await _userAuthenticationService.HandleGoogleLoginCallbackAsync(info);

            if (loginResult.Status != GoogleLoginStatus.Success || string.IsNullOrEmpty(loginResult.JwtToken) || loginResult.AppUser == null)
            {
                _logger.LogError("Google login håndtering i service fejlede med status: {Status}. Fejl: {ErrorMessage}", loginResult.Status, loginResult.ErrorMessage);
                return Redirect($"http://localhost:5173/login?error={HttpUtility.UrlEncode(loginResult.ErrorMessage ?? "Ukendt fejl ved Google login.")}");
            }
            
            _logger.LogInformation("Bruger {UserName} succesfuldt behandlet via Google. JWT genereret.", loginResult.AppUser.UserName);

            // Ryd den midlertidige eksterne cookie
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            await _signInManager.SignOutAsync();


            string frontendBaseUrl = _config["FrontendBaseUrl"] ?? "http://localhost:5173";
            string loginSuccessPathOnFrontend = "/login-success";

            var queryParams = new Dictionary<string, string?> { { "token", loginResult.JwtToken } };

            var sanitizedReturnUrl = _userAuthenticationService.SanitizeReturnUrl(returnUrl); 

            if (!string.IsNullOrEmpty(sanitizedReturnUrl) && sanitizedReturnUrl.StartsWith("/"))
            {
                queryParams.Add("originalReturnUrl", sanitizedReturnUrl);
            }
            else if (!string.IsNullOrEmpty(returnUrl)) 
            {
                _logger.LogWarning(
                    "Ignorerer ugyldig returnUrl ('{OriginalReturnUrl}') modtaget i HandleGoogleCallback for redirect til LoginSuccessPage.",
                     _userAuthenticationService.SanitizeReturnUrl(returnUrl) 
                );
            }
            
            string urlForLoginSuccessPage = QueryHelpers.AddQueryString(
                $"{frontendBaseUrl}{loginSuccessPathOnFrontend}",
                queryParams
            );

            _logger.LogInformation("Redirecter til frontend's LoginSuccessPage: {FinalUrl}", urlForLoginSuccessPage);
            return Redirect(urlForLoginSuccessPage);
        }

        // private async Task<string> GenerateJwtToken(User user)
        // {
        //     var jwtKey = _config["Jwt:Key"];
        //     if (string.IsNullOrEmpty(jwtKey))
        //     {
        //         throw new InvalidOperationException("JWT Key is not configured.");
        //     }
        //     var key = Encoding.UTF8.GetBytes(jwtKey);
        //     var userRoles = await _userManager.GetRolesAsync(user);

        //     var claims = new List<Claim>
        //     {
        //         new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        //         new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        //         new Claim(JwtRegisteredClaimNames.Name, user.UserName ?? ""),
        //         new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //         // Claims specifikke for ASP.NET Core Identity
        //         new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        //         new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
        //         // applikationsspecifikke claims
        //         new Claim("username", user.UserName ?? string.Empty),
        //         new Claim("userId", user.Id.ToString()),
        //     };

        //     foreach (var role in userRoles)
        //     {
        //         claims.Add(new Claim(ClaimTypes.Role, role));
        //     }

        //     var tokenDescriptor = new SecurityTokenDescriptor
        //     {
        //         Subject = new ClaimsIdentity(claims),
        //         Expires = DateTime.UtcNow.AddHours(1),
        //         Issuer = _config["Jwt:Issuer"],
        //         Audience = _config["Jwt:Audience"],
        //         SigningCredentials = new SigningCredentials(
        //             new SymmetricSecurityKey(key),
        //             SecurityAlgorithms.HmacSha256Signature
        //         ),
        //     };
        //     var tokenHandler = new JwtSecurityTokenHandler();
        //     var token = tokenHandler.CreateToken(tokenDescriptor);
        //     return tokenHandler.WriteToken(token);
        // }
    }
}
