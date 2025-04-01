using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RBACController : ControllerBase
    {
        private readonly DataContext _context;

        public RBACController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("PublicData")]
        [Authorize(Policy = "UserOrAdmin")]
        public IActionResult GetPublicData()
        {
            return Ok(new { Message = "This is accessible by Users and Admins" });
        }

        [HttpGet("AdminData")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public IActionResult GetAdminData()
        {
            var user = HttpContext.User; // Hent nuværende bruger
            var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList(); // Hent claims
            return Ok(new { Message = "This is accessible only by Admins", Claims = claims });
        }

        // PUT: api/rbac/addAdmin
        [HttpPut("addAdmin")]
        public async Task<IActionResult> AddAdmin([FromBody] string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email); // Find brugeren i databasen

            if (user == null)
                return NotFound(new { Message = "User not found" });

            if (!user.Roles.Contains("Admin"))
            {
                user.Roles.Add("Admin"); // Add amin rollen
                _context.Users.Update(user); // EF burde gøre dette automatisk
                await _context.SaveChangesAsync(); // Gem ændringerne i databasen
                return Ok(new { Message = "Added Admin role", Roles = user.Roles });
            }
            return Ok(new { Message = "User already has admin status" });
        }

        // PUT: api/rbac/removeAdmin
        [HttpPut("removeAdmin")]
        public async Task<IActionResult> removeAdmin([FromBody] string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
                return NotFound(new { Message = "User not found" });

            if (user.Roles.Contains("Admin"))
            {
                user.Roles.Remove("Admin"); // Fjerner admin rollen
                _context.Users.Update(user); // EF burde gøre dette automatisk
                await _context.SaveChangesAsync(); // Gem ændringerne i databasen
                return Ok(new { Message = "Removed Admin role", Roles = user.Roles });
            }
            return Ok(new { Message = "User doesn't have admin status" });
        }
    }
}
