using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using BCrypt.Net;
using System.Net.Http;
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

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        //private readonly DataContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;
        private readonly IHttpClientFactory _httpClientFactory; 
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        //private readonly IUrlHelper _urlHelper;

        public UsersController(
            IConfiguration config,
            EmailService emailService,
            IHttpClientFactory httpClientFactory,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _config = config;
            _emailService = emailService;
            _httpClientFactory = httpClientFactory; 
            _userManager = userManager;
            _signInManager = signInManager;
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
                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = HttpUtility.UrlEncode(token);
                var frontendBaseUrl = _config["FrontendBaseUrl"];

                var verificationLink = $"{frontendBaseUrl}/verify-email?userId={user.Id}&token={encodedToken}";

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
                    Console.WriteLine($"Fejl ved afsendelse af verification email til {user.Email}: {ex.Message}");
                }
                return Ok(new { message = "Registrering succesfuld! Tjek din email for at bekræfte din konto." });
            }
            else 
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { errors });
            }
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyEmail([FromQuery] int userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token)) {
                  return BadRequest("Token mangler.");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return BadRequest("Ugyldigt bruger ID.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if(result.Succeeded)
            {
                return Ok(new { message = "Email verificeret! Du kan nu logge ind." });
            }
            else 
            {
                Console.WriteLine($"Email verification failed for user {userId}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return BadRequest("Ugyldigt eller udløbet verifikationslink");
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

        [HttpGet("/auth/google/callback")] 
        public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string? state = null)
        {
            Console.WriteLine("Google Callback modtaget med kode."); 

            var clientId = _config["GoogleOAuth:ClientId"];
            var clientSecret = _config["GoogleOAuth:ClientSecret"];
            var redirectUri = "http://localhost:5218/auth/google/callback"; 

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                Console.WriteLine("FEJL: Google ClientId eller ClientSecret mangler i konfigurationen.");
                return BadRequest("Server konfigurationsfejl.");
            }

            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"code", code},
                {"client_id", clientId},
                {"client_secret", clientSecret},
                {"redirect_uri", redirectUri},
                {"grant_type", "authorization_code"}
            });

            var httpClient = _httpClientFactory.CreateClient(); 
            HttpResponseMessage tokenResponse;
            try
            {
                tokenResponse = await httpClient.PostAsync(tokenEndpoint, content);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"FEJL ved kommunikation med Google Token Endpoint: {ex.Message}");
                return StatusCode(502, "Fejl ved kommunikation med Google."); 
            }


            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"FEJL fra Google Token Endpoint: {tokenResponse.StatusCode} - {errorContent}");
                return BadRequest("Kunne ikke få token fra Google.");
            }

            GoogleTokenResponse? googleTokens;
            try
            {
                googleTokens = await tokenResponse.Content.ReadFromJsonAsync<GoogleTokenResponse>();
                if (googleTokens == null || string.IsNullOrEmpty(googleTokens.id_token))
                {
                    Console.WriteLine("FEJL: Kunne ikke deserialisere token respons eller id_token mangler.");
                    return BadRequest("Ugyldigt svar fra Google (token).");
                }
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                Console.WriteLine($"FEJL ved deserialisering af token respons: {jsonEx.Message}");
                return BadRequest("Ugyldigt svarformat fra Google (token).");
            }


            Console.WriteLine("Tokens modtaget fra Google.");

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwtToken = null;
            try
            {
                jwtToken = handler.ReadJwtToken(googleTokens.id_token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FEJL ved læsning af id_token: {ex.Message}");
                return BadRequest("Ugyldigt id_token format fra Google.");
            }


            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");
            var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "name"); 
            var googleUserIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub"); // Googles unikke bruger ID

            if (emailClaim == null || googleUserIdClaim == null)
            {
                return BadRequest("Kunne ikke finde email i Google token.");
            }
            var userEmail = emailClaim.Value;
            var googleUserId = googleUserIdClaim.Value; 
            var userName = nameClaim?.Value ?? userEmail.Split('@')[0]; 


            Console.WriteLine($"Brugerinfo fra id_token: Email={userEmail}, Name={userName}");

            var loginInfo = new UserLoginInfo("Google", googleUserId, "Google"); 
            var user = await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);

            if (user == null)
            {
                // Bruger ikke fundet via Google login, tjek om emailen findes
                user = await _userManager.FindByEmailAsync(userEmail);

                if (user == null)
                {
                    // Bruger findes slet ikke - Opret ny bruger
                    Console.WriteLine($"Bruger med email {userEmail} findes ikke. Opretter ny.");
                    user = new User
                    {
                        UserName = userName,
                        Email = userEmail,
                        EmailConfirmed = true, 
                        
                    };
                    var createUserResult = await _userManager.CreateAsync(user);
                    if (!createUserResult.Succeeded)
                    {
                        Console.WriteLine($"FEJL ved oprettelse af Identity bruger: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
                        return BadRequest("Kunne ikke oprette brugerkonto."); 
                    }
                    await _userManager.AddToRoleAsync(user, "User"); 

                    Console.WriteLine($"Ny bruger oprettet med Id: {user.Id}");
                }

                var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
                if (!addLoginResult.Succeeded)
                {
                    Console.WriteLine($"FEJL ved tilføjelse af Google login til bruger {user.Id}: {string.Join(", ", addLoginResult.Errors.Select(e => e.Description))}");
                    return BadRequest("Kunne ikke linke Google konto.");
                }
                Console.WriteLine($"Google login linket til bruger Id: {user.Id}");
            }
            else
            {
                Console.WriteLine($"Bruger fundet via Google login med Id: {user.Id}");
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user); 
                }
            }
            var localJwtToken = await GenerateJwtToken(user); 
            Console.WriteLine("Lokal JWT genereret.");

            var frontendLoginSuccessUrl = $"http://localhost:5173/login-success?token={localJwtToken}"; // Eller direkte til /home?

            Console.WriteLine($"Redirecter til frontend: {frontendLoginSuccessUrl}");
            return Redirect(frontendLoginSuccessUrl);
        }
    

        // Helper klasse til at deserialisere Google's token svar
        private class GoogleTokenResponse
        {
            public string? access_token { get; set; }
            public string? id_token { get; set; }
            public int expires_in { get; set; }
            public string? token_type { get; set; }
            public string? scope { get; set; }
            public string? refresh_token { get; set; } // Fås kun hvis du anmoder om offline access
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