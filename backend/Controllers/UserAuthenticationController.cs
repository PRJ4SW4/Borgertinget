using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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

        public UsersController(
            DataContext context,
            IConfiguration config,
            EmailService emailService
        )
        {
            _context = context;
            _config = config;
            _emailService = emailService;
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
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(new { error = errors });
            }

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
            var verificationLink = $"http://localhost:5173/verify?token={verificationToken}";
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
            Console.WriteLine("Inde i verify");
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.VerificationToken.ToLower() == token.ToLower()
            );

            if (user == null)
                return BadRequest("Ugyldigt eller udløbet verifikationslink.");

            Console.WriteLine("Bruger fundet");
            user.IsVerified = true;
            Console.WriteLine("Vi er forbi IsVerified");
            user.VerificationToken = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Email verificeret! Du kan nu logge ind." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // 1. Find bruger ud fra E-mail eller brugernavn
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == dto.EmailOrUsername || u.UserName == dto.EmailOrUsername
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

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
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
