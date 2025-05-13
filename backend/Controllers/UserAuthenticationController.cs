using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using BCrypt.Net;
using System.Net.Http;
using System.Linq;
using System.Net;
using backend.Services;
using backend.DTOs;
using backend.Models;
using backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Web;
using backend.utils;
using CoreTweet.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication; 
using Microsoft.AspNetCore.Authentication.Google;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        //private readonly DataContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;
        //private readonly IHttpClientFactory _httpClientFactory; 
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IConfiguration config,
            EmailService emailService,
            //IHttpClientFactory httpClientFactory,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<UsersController> logger)
        {
            _config = config;
            _emailService = emailService;
            //_httpClientFactory = httpClientFactory; 
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
            var user = new User
            {
                UserName = dto.Username,
                Email = dto.Email,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            
            if (result.Succeeded) {
                // var roleResult = await _userManager.AddToRoleAsync(user, "User");
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                _logger.LogInformation($"Token: {token}");
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                _logger.LogInformation($"Encoded Token: {encodedToken}");

                var verificationLink = $"http://localhost:5173/verify?userId={user.Id}&token={encodedToken}";

                var subject = "Bekræft din e-mailadresse";
                var message = $@"
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
                    _logger.LogError(ex, $"Fejl ved afsendelse af bekræftelsesmail til {user.Email}: {ex.Message}");
                    return StatusCode(500, new { message = "Fejl ved afsendelse af bekræftelsesmail. Prøv venligst igen senere." } );
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
        public async Task<IActionResult> VerifyEmail([FromQuery] int userId, [FromQuery] string token)
        {
            _logger.LogInformation($"Token: {token}");
            if (string.IsNullOrEmpty(token)) {
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
                _logger.LogInformation($"Decoderet token: {decodedToken}");
            
                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
                if(result.Succeeded)
                {
                    return Ok(new { message = "Din emailadresse er bekræftet. Du kan nu logge ind." });
                }
                else 
                {
                    Console.WriteLine($"Email verification failed for user {userId}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    return BadRequest("Ugyldigt eller udløbet verifikationslink");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fejl ved afkodning af token");
                return BadRequest(new {message = "Ugyldigt token format"});
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
                return BadRequest(new { error = "Bruger findes ikke." });
            
            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);

            if(result.Succeeded) {
                var token = GenerateJwtToken(user);
                return Ok(new { token });
            }
            else if (result.IsNotAllowed)
            {
                return BadRequest(new { error = "Din emailadresse er ikke blevet bekræftet. Tjek din indbakke for at bekræfte."});
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
            // Den URL, vores egen HandleGoogleCallback lytter på.
            // clientReturnUrl er den URL, vi vil sende brugeren til på frontend EFTER HELE processen.
            string backendCallbackWithClientReturnUrl = Url.Action(
                nameof(HandleGoogleCallback), 
                "Users", // Controller navnet uden "Controller" suffiks
                new { returnUrl = clientReturnUrl }, // Query parametre til VORES callback
                Request.Scheme // http eller https
            );

            // Hvis Url.Action fejler (f.eks. pga. routing ikke er fuldt initialiseret endnu under opstart hvis kaldt for tidligt),
            // kan du have en fallback, men det burde virke i en normal controller action.
            if (string.IsNullOrEmpty(backendCallbackWithClientReturnUrl)) {
                _logger.LogError("Kunne ikke generere URL til HandleGoogleCallback. Undersøg routing opsætning.");
                return BadRequest("Intern serverfejl ved generering af callback URL.");
            }

            _logger.LogInformation("LoginWithGoogle: Den 'redirectUri' der konfigureres for Google-handleren (via Properties) er: {BackendCallback}", backendCallbackWithClientReturnUrl);

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                GoogleDefaults.AuthenticationScheme,
                backendCallbackWithClientReturnUrl // Dette er den URL, som OnTicketReceived vil modtage i ctx.Properties.RedirectUri
            );

            // Challenge vil bruge den options.CallbackPath (/signin-google) der er sat i AddGoogle()
            // til at fortælle Google, hvor Google skal redirecte hen.
            // Den redirectUri vi lige har sat i 'properties' bruges internt af Identity til
            // at vide hvor den skal hen EFTER /signin-google er ramt og behandlet.
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("HandleGoogleCallback")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleGoogleCallback([FromQuery] string? returnUrl = null, [FromQuery] string? remoteError = null)
        {
            _logger.LogInformation("Modtaget callback fra Google."); //

            if (!string.IsNullOrEmpty(remoteError))
            {
                _logger.LogError($"Fejl fra ekstern udbyder: {remoteError}"); //
                return Redirect($"http://localhost:5173/login?error={HttpUtility.UrlEncode(remoteError)}");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogError("Kunne ikke hente ekstern login information."); //
                return Redirect($"http://localhost:5173/login?error={HttpUtility.UrlEncode("Fejl ved eksternt login.")}");
            }

            // Log brugeren ind med den eksterne login udbyder.
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            User appUser;
            if (signInResult.Succeeded)
            {
                appUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (appUser == null) {
                    _logger.LogError($"Bruger ikke fundet med FindByLoginAsync efter succesfuld ExternalLoginSignInAsync for {info.LoginProvider} - {info.ProviderKey}."); //
                    return Redirect($"http://localhost:5173/login?error={HttpUtility.UrlEncode("Bruger konto problem.")}");
                }
                _logger.LogInformation($"Bruger {appUser.UserName} logget ind med {info.LoginProvider}."); //
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogError("Email claim ikke fundet i eksternt principal."); //
                    return Redirect($"http://localhost:5173/login?error={HttpUtility.UrlEncode("Email ikke modtaget fra Google.")}");
                }

                appUser = await _userManager.FindByEmailAsync(email);
                if (appUser == null) // Opret ny lokal bruger
                {
                    // Hent navnet fra Google. info.Principal.FindFirstValue(ClaimTypes.Name) er ofte det fulde navn.
                    var nameFromGoogle = info.Principal.FindFirstValue(ClaimTypes.Name);
                    var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName); // Fornavn
                    var surname = info.Principal.FindFirstValue(ClaimTypes.Surname);   // Efternavn
                    
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
                    var sanitizedUserName = new string(baseUserName.Where(char.IsLetterOrDigit).ToArray());

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

                    _logger.LogInformation("Forsøger at oprette ny bruger med UserName: {UserName} (sanitized from Google) og Email: {Email}", sanitizedUserName, email);
                    appUser = new User { UserName = sanitizedUserName, Email = email, EmailConfirmed = true }; 
                    var createUserResult = await _userManager.CreateAsync(appUser);
                    if (!createUserResult.Succeeded)
                    {
                        _logger.LogError($"Fejl ved oprettelse af bruger ({email}): {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
                        // Videresend den første Identity fejl til frontend for mere specifik feedback
                        string errorDetail = createUserResult.Errors.FirstOrDefault()?.Description ?? "Kunne ikke oprette bruger.";
                        return Redirect($"http://localhost:5173/login?error={HttpUtility.UrlEncode(errorDetail)}");
                    }
                }
                else
                {
                    if (!appUser.EmailConfirmed) // Bekræft emailen hvis den ikke allerede er det
                    {
                        appUser.EmailConfirmed = true;
                        await _userManager.UpdateAsync(appUser);
                        _logger.LogInformation($"Email bekræftet for eksisterende bruger: {appUser.UserName}"); //
                    }
                }

                // Link Google login til den lokale brugerkonto
                var addLoginResult = await _userManager.AddLoginAsync(appUser, info);
                if (!addLoginResult.Succeeded)
                {
                    _logger.LogError($"Fejl ved at linke eksternt login: {string.Join(", ", addLoginResult.Errors.Select(e => e.Description))}"); //
                    return Redirect($"http://localhost:5173/login?error={HttpUtility.UrlEncode("Kunne ikke linke Google konto.")}");
                }
                _logger.LogInformation($"Eksternt login linket for bruger: {appUser.UserName}"); //
            }
            
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var localJwtToken = await GenerateJwtToken(appUser); // Sørg for at GenerateJwtToken er async eller kald den korrekt
            _logger.LogInformation($"JWT genereret for bruger: {appUser.UserName}"); //

            string frontendBaseUrl = _config["FrontendBaseUrl"] ?? "http://localhost:5173"; 
            string successRedirectPath = "/login-success"; 

            string redirectTarget;
            if (!string.IsNullOrEmpty(returnUrl) /*&& IsLocalUrl(returnUrl)*/) 
            {
                redirectTarget = $"{frontendBaseUrl}{returnUrl}";
            }
            else
            {
                // Default redirect, hvis returnUrl ikke er gyldig eller ikke angivet.
                // Peger på din /login-success side på frontend.
                redirectTarget = $"{frontendBaseUrl}{successRedirectPath}";
            }

            string finalFrontendRedirectUrl = QueryHelpers.AddQueryString(redirectTarget, "token", HttpUtility.UrlEncode(localJwtToken));

            _logger.LogInformation($"Redirecter til frontend: {finalFrontendRedirectUrl}");
            return Redirect(finalFrontendRedirectUrl);
        }


        // [HttpGet("/auth/google/callback")] 
        // public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string? state = null)
        // {
        //     Console.WriteLine("Google Callback modtaget med kode."); 

        //     var clientId = _config["GoogleOAuth:ClientId"];
        //     var clientSecret = _config["GoogleOAuth:ClientSecret"];
        //     var redirectUri = "http://localhost:5218/auth/google/callback"; 

        //     if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        //     {
        //         Console.WriteLine("FEJL: Google ClientId eller ClientSecret mangler i konfigurationen.");
        //         return BadRequest("Server konfigurationsfejl.");
        //     }

        //     var tokenEndpoint = "https://oauth2.googleapis.com/token";
        //     var content = new FormUrlEncodedContent(new Dictionary<string, string>
        //     {
        //         {"code", code},
        //         {"client_id", clientId},
        //         {"client_secret", clientSecret},
        //         {"redirect_uri", redirectUri},
        //         {"grant_type", "authorization_code"}
        //     });

        //     var httpClient = _httpClientFactory.CreateClient(); 
        //     HttpResponseMessage tokenResponse;
        //     try
        //     {
        //         tokenResponse = await httpClient.PostAsync(tokenEndpoint, content);
        //     }
        //     catch (HttpRequestException ex)
        //     {
        //         Console.WriteLine($"FEJL ved kommunikation med Google Token Endpoint: {ex.Message}");
        //         return StatusCode(502, "Fejl ved kommunikation med Google."); 
        //     }


        //     if (!tokenResponse.IsSuccessStatusCode)
        //     {
        //         var errorContent = await tokenResponse.Content.ReadAsStringAsync();
        //         Console.WriteLine($"FEJL fra Google Token Endpoint: {tokenResponse.StatusCode} - {errorContent}");
        //         return BadRequest("Kunne ikke få token fra Google.");
        //     }

        //     GoogleTokenResponse? googleTokens;
        //     try
        //     {
        //         googleTokens = await tokenResponse.Content.ReadFromJsonAsync<GoogleTokenResponse>();
        //         if (googleTokens == null || string.IsNullOrEmpty(googleTokens.id_token))
        //         {
        //             Console.WriteLine("FEJL: Kunne ikke deserialisere token respons eller id_token mangler.");
        //             return BadRequest("Ugyldigt svar fra Google (token).");
        //         }
        //     }
        //     catch (System.Text.Json.JsonException jsonEx)
        //     {
        //         Console.WriteLine($"FEJL ved deserialisering af token respons: {jsonEx.Message}");
        //         return BadRequest("Ugyldigt svarformat fra Google (token).");
        //     }


        //     Console.WriteLine("Tokens modtaget fra Google.");

        //     var handler = new JwtSecurityTokenHandler();
        //     JwtSecurityToken? jwtToken = null;
        //     try
        //     {
        //         jwtToken = handler.ReadJwtToken(googleTokens.id_token);
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"FEJL ved læsning af id_token: {ex.Message}");
        //         return BadRequest("Ugyldigt id_token format fra Google.");
        //     }


        //     var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");
        //     var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "name"); 
        //     var googleUserIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub"); // Googles unikke bruger ID

        //     if (emailClaim == null || googleUserIdClaim == null)
        //     {
        //         return BadRequest("Kunne ikke finde email i Google token.");
        //     }
        //     var userEmail = emailClaim.Value;
        //     var googleUserId = googleUserIdClaim.Value; 
        //     var userName = nameClaim?.Value ?? userEmail.Split('@')[0]; 


        //     Console.WriteLine($"Brugerinfo fra id_token: Email={userEmail}, Name={userName}");

        //     var loginInfo = new UserLoginInfo("Google", googleUserId, "Google"); 
        //     var user = await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);

        //     if (user == null)
        //     {
        //         // Bruger ikke fundet via Google login, tjek om emailen findes
        //         user = await _userManager.FindByEmailAsync(userEmail);

        //         if (user == null)
        //         {
        //             // Bruger findes slet ikke - Opret ny bruger
        //             Console.WriteLine($"Bruger med email {userEmail} findes ikke. Opretter ny.");
        //             user = new User
        //             {
        //                 UserName = userName,
        //                 Email = userEmail,
        //                 EmailConfirmed = true, 
                        
        //             };
        //             var createUserResult = await _userManager.CreateAsync(user);
        //             if (!createUserResult.Succeeded)
        //             {
        //                 Console.WriteLine($"FEJL ved oprettelse af Identity bruger: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
        //                 return BadRequest("Kunne ikke oprette brugerkonto."); 
        //             }
        //             await _userManager.AddToRoleAsync(user, "User"); 

        //             Console.WriteLine($"Ny bruger oprettet med Id: {user.Id}");
        //         }

        //         var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
        //         if (!addLoginResult.Succeeded)
        //         {
        //             Console.WriteLine($"FEJL ved tilføjelse af Google login til bruger {user.Id}: {string.Join(", ", addLoginResult.Errors.Select(e => e.Description))}");
        //             return BadRequest("Kunne ikke linke Google konto.");
        //         }
        //         Console.WriteLine($"Google login linket til bruger Id: {user.Id}");
        //     }
        //     else
        //     {
        //         Console.WriteLine($"Bruger fundet via Google login med Id: {user.Id}");
        //         if (!await _userManager.IsEmailConfirmedAsync(user))
        //         {
        //             user.EmailConfirmed = true;
        //             await _userManager.UpdateAsync(user); 
        //         }
        //     }
        //     var localJwtToken = await GenerateJwtToken(user); 
        //     Console.WriteLine("Lokal JWT genereret.");

        //     var frontendLoginSuccessUrl = $"http://localhost:5173/login-success?token={localJwtToken}"; // Eller direkte til /home?

        //     Console.WriteLine($"Redirecter til frontend: {frontendLoginSuccessUrl}");
        //     return Redirect(frontendLoginSuccessUrl);
        // }
    

        // Helper klasse til at deserialisere Google's token svar
        // private class GoogleTokenResponse
        // {
        //     public string? access_token { get; set; }
        //     public string? id_token { get; set; }
        //     public int expires_in { get; set; }
        //     public string? token_type { get; set; }
        //     public string? scope { get; set; }
        //     public string? refresh_token { get; set; } // Fås kun hvis du anmoder om offline access
        // }

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
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // "Subject" - Hvem tokenet handler om (brugerens ID).  
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), 

                // Claims specifikke for ASP.NET Core Identity
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty), 

                // applikationsspecifikke claims
                new Claim("username", user.UserName ?? string.Empty), 
                new Claim("userId", user.Id.ToString()) 
            };

            foreach (var role in user.Roles)
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
                )
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}