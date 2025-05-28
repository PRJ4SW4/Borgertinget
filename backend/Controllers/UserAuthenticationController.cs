using System.Text;
using System.Web;
using backend.DTOs;
using backend.Models;
using backend.Services.Authentication;
using backend.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase 
    {
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly ILogger<UsersController> _logger; 
        private readonly IUserAuthenticationService _userAuthenticationService;

        public UsersController(
            IConfiguration config,
            IEmailService emailService,
            ILogger<UsersController> logger,
            IUserAuthenticationService userAuthenticationService
        )
        {
            _config = config;
            _emailService = emailService;
            _logger = logger;
            _userAuthenticationService = userAuthenticationService;
        }

        // POST: api/users
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] RegisterUserDto dto)
        {
            var result = await _userAuthenticationService.CreateUserAsync(dto);

            if (result.Succeeded)
            {
                // Leder efter den oprettede bruger for at tildele en rolle
                var user = await _userAuthenticationService.FindUserByEmailAsync(dto.Email);
                if (user == null)
                {
                    return BadRequest(new { error = "Bruger blev ikke oprettet." });
                }

                var roleResult = await _userAuthenticationService.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                {
                    var errors = roleResult.Errors.Select(e => e.Description);
                    _logger.LogError(
                        $"Fejl ved tildeling af rolle 'User' til {LogSanitizer.Sanitize(user.UserName)}: {string.Join(", ", errors)}"
                    );
                    await _userAuthenticationService.DeleteUserAsync(user);

                    return StatusCode(
                        500,
                        new
                        {
                            message = "Brugeren blev oprettet, men der opstod en fejl ved tildeling af rolle.",
                            errors,
                        }
                    );
                }
                // EmailService genererer en bekræftelsesmail med et token.
                var token = await _userAuthenticationService.GenerateEmailConfirmationTokenAsync(
                    user
                );
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var emailContent = _emailService.GenerateRegistrationEmailAsync(encodedToken, user);

                try
                {
                    // Afsender bekræftelsesmailen med SMTP.
                    await _emailService.SendEmailAsync(dto.Email, emailContent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Fejl ved afsendelse af bekræftelsesmail: {ex.Message}");
                    return StatusCode(
                        500,
                        new { message = "Fejl ved afsendelse af mail. Prøv venligst igen senere." }
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
        [AllowAnonymous]
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
            // Service leder efter en bruger med det angivne ID.
            var user = await _userAuthenticationService.GetUserAsync(userId);
            if (user == null)
            {
                return BadRequest("Ugyldigt bruger ID.");
            }

            try
            {
                // Token fra URL dekodes
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);
                var result = await _userAuthenticationService.ConfirmEmailAsync(user, decodedToken);

                if (!result.Succeeded)
                {
                    _logger.LogError(
                        $"Email verification failed for user {userId}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                    );
                    return BadRequest("Ugyldigt eller udløbet verifikationslink");
                }
                else
                {
                    return Ok(
                        new { message = "Din emailadresse er bekræftet. Du kan nu logge ind." }
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fejl ved afkodning af token");
                return BadRequest(new { message = "Ugyldigt token format" });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            string loginInput = dto.EmailOrUsername.ToLower();
            User? user;

            // Finder bruger ud fra E-mail eller brugernavn
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

            var result = await _userAuthenticationService.CheckPasswordSignInAsync(
                user,
                dto.Password,
                false
            );

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
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _userAuthenticationService.FindUserByEmailAsync(dto.Email);
            if (user == null)
            {
                return BadRequest(new { error = "Bruger findes ikke." });
            }

            // Token genereres for at nulstille adgangskoden.
            var token = await _userAuthenticationService.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var emailContent = _emailService.GenerateResetPasswordEmailAsync(encodedToken, user);

            try
            {
                // Afsender nulstillingsmailen med SMTP.
                await _emailService.SendEmailAsync(dto.Email, emailContent);
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
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordDto dto,
            [FromQuery] int userId,
            [FromQuery] string token
        )
        {
            // Leder efter en bruger med det angivne ID.
            var user = await _userAuthenticationService.GetUserAsync(userId);
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
                // Token fra URL dekodes
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
                var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userAuthenticationService.ResetPasswordAsync(
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

        [HttpGet("login-google")]
        [AllowAnonymous]
        public IActionResult LoginWithGoogle([FromQuery] string? clientReturnUrl = null)
        {
            var sanitizedClientReturnUrl = _userAuthenticationService.SanitizeReturnUrl(
                clientReturnUrl
            );

            _logger.LogInformation(
                "Start Google login process. Ønsket frontend returnUrl for efterfølgende redirect: {ClientReturnUrl}",
                sanitizedClientReturnUrl
            );

            // Her bygger vi den URL, som Google skal sende brugeren tilbage til.
            // Den peger på vores egen HandleGoogleCallback metode 
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
            // Her bygger vi en pakke med instrukser, som ASP.NET Identity skal bruge til at håndtere login-processen
            // Den indeholder vigtigdt vores 'RedirectUri', 
            // så Google ved, at de skal sende brugeren tilbage til vores HandleGoogleCallback-metode
            var authenticationProperties =
                _userAuthenticationService.ConfigureExternalAuthenticationProperties(
                    GoogleDefaults.AuthenticationScheme,
                    propertiesRedirectUri 
                );
            // Her sender vi brugeren afsted til Google for at logge ind.
            return Challenge(authenticationProperties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("HandleGoogleCallback")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleGoogleCallback(
            [FromQuery] string? returnUrl = null,
            [FromQuery] string? remoteError = null
        )
        {
            _logger.LogInformation("Modtaget callback fra Google i controller.");

            if (!string.IsNullOrEmpty(remoteError))
            {
                var sanitizedRemoteError = _userAuthenticationService.SanitizeReturnUrl(
                    remoteError
                );
                _logger.LogError(
                    "Fejl fra ekstern udbyder (Google): {RemoteError}",
                    sanitizedRemoteError
                );
                return Redirect(
                    $"http://localhost:5173/login?error={HttpUtility.UrlEncode(remoteError)}"
                );
            }

            // Her pakker vi bruger-informationen ud som vi fik fra Google
            var info = await _userAuthenticationService.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Kunne ikke hente ekstern login information i controlleren.");
                return Redirect(
                    $"http://localhost:5173/login?error={HttpUtility.UrlEncode("Fejl ved eksternt login.")}"
                );
            }

            var loginResult = await _userAuthenticationService.HandleGoogleLoginCallbackAsync(info);

            if (
                loginResult.Status != GoogleLoginStatus.Success
                || string.IsNullOrEmpty(loginResult.JwtToken)
                || loginResult.AppUser == null
            )
            {
                return Redirect(
                    $"http://localhost:5173/login?error={HttpUtility.UrlEncode(loginResult.ErrorMessage ?? "Ukendt fejl ved Google login.")}"
                );
            }

            // Her rydder vi op ved at slette de midlertidige cookies fra login-processen
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            await _userAuthenticationService.SignOutAsync();

            string frontendBaseUrl = _config["FrontendBaseUrl"] ?? "http://localhost:5173";
            string loginSuccessPathOnFrontend = "/login-success";

            // Her samler vi de ting, der skal med i URL'en, f.eks. det nye token
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

            // Her bygger vi den endelige URL til vores frontend
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
    }
}
