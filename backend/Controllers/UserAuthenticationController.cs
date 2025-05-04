using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;
        private readonly IHttpClientFactory _httpClientFactory;

        public UsersController(
            DataContext context,
            IConfiguration config,
            EmailService emailService,
            IHttpClientFactory httpClientFactory
        )
        {
            _context = context;
            _config = config;
            _emailService = emailService;
            _httpClientFactory = httpClientFactory;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // POST: api/users
        [HttpPost("register")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterUserDto dto)
        {
            // 1. Check om brugernavn eller email allerede findes
            var existingUserName = await _context.Users.AnyAsync(u => u.UserName == dto.Username);

            var existingEmail = await _context.Users.AnyAsync(u => u.Email == dto.Email);

            var existingEmailAndUserName = await _context.Users.AnyAsync(u =>
                u.Email == dto.Email && u.UserName == dto.Username
            );

            if (existingEmailAndUserName)
                return BadRequest(new { error = "Brugernavn og email er allerede i brug." });

            if (existingUserName)
                return BadRequest(new { error = "Brugernavn er allerede i brug." });

            if (existingEmail)
                return BadRequest(new { error = "Email er allerede i brug." });

            if (string.IsNullOrEmpty(dto.Password))
                return BadRequest(new { error = "Adgangskode er påkrævet." });

            if (!Regex.IsMatch(dto.Password, @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$"))
                return BadRequest(
                    new
                    {
                        error = "Adgangskode skal have mindst 8 tegn, et stort og et lille bogstav, samt et tal.",
                    }
                );

            // 2. Hash password med BCrypt
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 3. Opret bruger-objekt
            var verificationToken = Guid.NewGuid().ToString();
            var user = new User
            {
                UserName = dto.Username,
                Email = dto.Email,
                PasswordHash = hashedPassword,
                VerificationToken = verificationToken,
                Roles = new List<string> { "User" },
            };

            // 4. Gem i databasen
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 5. Send verifikations-email
            var verificationLink = $"http://localhost:5173/login?token={verificationToken}";
            _emailService.SendVerificationEmail(user.Email, verificationLink);

            // Returnér en DTO eller blot ID/brugernavn
            return Ok(
                new
                {
                    message = "Registrering succesfuld",
                    user.Id,
                    user.UserName,
                    user.Email,
                }
            );
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            Console.WriteLine("Received token: " + token);

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.VerificationToken != null &&
                u.VerificationToken.ToLower() == token.ToLower()
            );

            if (user == null)
            {
                // Maybe the user is already verified?
                var alreadyVerified = await _context.Users.FirstOrDefaultAsync(u => u.IsVerified && u.VerificationToken == null);
                if (alreadyVerified != null)
                {
                    return Ok(new { message = "Email er allerede blevet verificeret." });
                }

                return BadRequest("Ugyldigt eller udløbet verifikationslink.");
            }

            Console.WriteLine("User found: " + user.Email);

            user.IsVerified = true;
            user.VerificationToken = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Email verificeret! Du kan nu logge ind." });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            
            string loginInput = dto.EmailOrUsername.ToLower();

            // 1. Find bruger ud fra E-mail eller brugernavn
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email.ToLower() == loginInput || u.UserName.ToLower() == loginInput
            );

            if (user == null)
                return BadRequest(new { error = "Bruger findes ikke" });

            // 2. Sammenlign indtastet password med gemt hash
            var isMatch = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isMatch)
                return BadRequest(new { error = "Forkert adgangskode" });

            // 3. Tjek om email er verificeret
            if (!user.IsVerified)
                return BadRequest(new { error = "Email er ikke verificeret" });

            // 4. Ved succes login – evt. generér en JWT token eller lignende
            // (Her bare et eksempel)
            // return Ok(new { message = "Login succesfuldt", user.Id, user.UserName });
            var token = GenerateJwtToken(user); // Du skal implementere denne metode
            return Ok(new { token });
        }

        [HttpGet("/auth/google/callback")] // Eller en anden route der matcher din sti
        public async Task<IActionResult> GoogleCallback(
            [FromQuery] string code,
            [FromQuery] string? state = null
        )
        {
            Console.WriteLine("Google Callback modtaget med kode."); // Til debugging

            // ----- Trin 3.1: Hent Google Credentials -----
            var clientId = _config["GoogleOAuth:ClientId"];
            var clientSecret = _config["GoogleOAuth:ClientSecret"];
            var redirectUri = "http://localhost:5218/auth/google/callback";

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                // Log en fejl - konfiguration mangler
                Console.WriteLine(
                    "FEJL: Google ClientId eller ClientSecret mangler i konfigurationen."
                );
                return BadRequest("Server konfigurationsfejl."); // Undgå at afsløre for meget
            }

            // ----- (Valgfrit, men anbefalet): Valider 'state' her, hvis du implementerer det -----
            // if (!isValidState(state)) return BadRequest("Invalid state parameter.");

            // ----- Trin 3.2: Byt 'code' til tokens hos Google -----
            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            var content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    { "code", code },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "redirect_uri", redirectUri },
                    { "grant_type", "authorization_code" },
                }
            );

            var httpClient = _httpClientFactory.CreateClient(); // Få en HttpClient
            HttpResponseMessage tokenResponse;
            try
            {
                tokenResponse = await httpClient.PostAsync(tokenEndpoint, content);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"FEJL ved kommunikation med Google Token Endpoint: {ex.Message}"
                );
                return StatusCode(502, "Fejl ved kommunikation med Google."); // Bad Gateway
            }

            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                Console.WriteLine(
                    $"FEJL fra Google Token Endpoint: {tokenResponse.StatusCode} - {errorContent}"
                );
                return BadRequest("Kunne ikke få token fra Google.");
            }

            // Læs og deserialiser svaret (access_token, id_token osv.)
            // Vi skal bruge System.Text.Json
            GoogleTokenResponse? googleTokens;
            try
            {
                googleTokens = await tokenResponse.Content.ReadFromJsonAsync<GoogleTokenResponse>();
                if (googleTokens == null || string.IsNullOrEmpty(googleTokens.id_token))
                {
                    Console.WriteLine(
                        "FEJL: Kunne ikke deserialisere token respons eller id_token mangler."
                    );
                    return BadRequest("Ugyldigt svar fra Google (token).");
                }
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                Console.WriteLine($"FEJL ved deserialisering af token respons: {jsonEx.Message}");
                return BadRequest("Ugyldigt svarformat fra Google (token).");
            }

            Console.WriteLine("Tokens modtaget fra Google.");

            // ----- Trin 3.3: Få brugerinformation (via id_token) -----
            // IMPORTANT: I produktion SKAL du validere signaturen på id_token!
            // Her nøjes vi med at parse det for at få claims (til læringsformål).
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
            var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "name"); // Eller "given_name" / "family_name"
            // var googleUserIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub"); // Googles unikke bruger ID

            if (emailClaim == null)
            {
                return BadRequest("Kunne ikke finde email i Google token.");
            }
            var userEmail = emailClaim.Value;
            var userName = nameClaim?.Value ?? userEmail.Split('@')[0]; // Brug navn hvis muligt, ellers del af email

            Console.WriteLine($"Brugerinfo fra id_token: Email={userEmail}, Name={userName}");

            // ----- Trin 3.4: Håndtér bruger i databasen -----
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                // Bruger findes ikke - Opret ny bruger
                Console.WriteLine($"Bruger med email {userEmail} findes ikke. Opretter ny.");
                user = new User
                {
                    Email = userEmail,
                    // Brugernavn: Overvej om 'name' fra Google er bedre/mere unikt end email-prefix
                    UserName = userName, // Eller find en bedre strategi for brugernavn
                    PasswordHash = "", // Ingen adgangskode sat via Google
                    IsVerified = true, // Google har verificeret emailen
                    VerificationToken = null, // Ingen grund til vores egen verificering
                    // Overvej at tilføje en kolonne til GoogleId (fra 'sub' claim) i User modellen
                };
                _context.Users.Add(user);
                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Ny bruger oprettet med Id: {user.Id}");
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine(
                        $"FEJL ved oprettelse af bruger i DB: {dbEx.InnerException?.Message ?? dbEx.Message}"
                    );
                    return StatusCode(500, "Fejl ved gemning af brugerdata.");
                }
            }
            else
            {
                Console.WriteLine($"Bruger fundet med Id: {user.Id}");
                // Opdater evt. brugerinfo hvis nødvendigt
                if (!user.IsVerified) // Hvis de tidligere har registreret men ikke verificeret
                {
                    user.IsVerified = true;
                    user.VerificationToken = null;
                    await _context.SaveChangesAsync();
                }
            }
            // ----- Trin 3.5: Log brugeren ind (Generér DIN JWT) -----
            var localJwtToken = GenerateJwtToken(user); // Genbruger din eksisterende metode
            Console.WriteLine("Lokal JWT genereret.");

            // ----- Trin 3.6: Redirect til Frontend med token -----
            // Send token som query parameter. Frontend skal håndtere dette.
            var frontendLoginSuccessUrl =
                $"http://localhost:5173/login-success?token={localJwtToken}"; // Eller direkte til /home?
            // OBS: Overvej sikkerheden ved at sende token i URL. Fragment (#) er lidt bedre.
            // Bedste løsning: Sæt en httpOnly cookie server-side, eller redirect til en side
            // hvor frontend laver et efterfølgende kald for at hente token.

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

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key is not configured.");
            }
            var key = Encoding.UTF8.GetBytes(jwtKey);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("userId", user.Id.ToString()),
            };

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256
                )
            );

            // ✅ Dette returnerer en **gyldig JWT string**
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            Console.WriteLine("Generated Token: " + tokenString); // Debugging
            return tokenString;
        }
    }
}
