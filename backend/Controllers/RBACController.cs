using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using backend.Data;
using backend.DTOs;
using backend.Models;
using backend.Services;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RBACController : ControllerBase
    {
        //private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public RBACController(
            UserManager<User> userManager,
            RoleManager<IdentityRole<int>> roleManager
        )
        {
            //_context = context;
            _userManager = userManager;
            _roleManager = roleManager;
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
        public async Task<IActionResult> AddAdmin([FromBody] UserEmailDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email); // Find brugeren i databasen

            if (user == null)
                return NotFound(new { Message = "Bruger findes ikke." });

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var result = await _userManager.AddToRoleAsync(user, "Admin");
                if (result.Succeeded)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    return Ok(
                        new { Message = "Administratorrolle tilføjet til bruger.", Roles = roles }
                    );
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }

            return Ok(new { Message = "Bruger har allerede administratorrolle." });

            // if (result.Succeeded)
            // {
            //     return Ok(new { Message = "Added Admin role", Email = user.Email });
            // }
            // else
            // {
            //     return BadRequest(new { Message = "Failed to add admin role", Errors = result.Errors.Select(e => e.Description) });
            // }

            // if (!user.Roles.Contains("Admin"))
            // {
            //     user.Roles.Add("Admin"); // Add amin rollen
            //     _context.Users.Update(user); // EF burde gøre dette automatisk
            //     await _context.SaveChangesAsync(); // Gem ændringerne i databasen
            //     return Ok(new { Message = "Added Admin role", Roles = user.Roles });
            // }
            // return Ok(new { Message = "User already has admin status" });
        }

        // PUT: api/rbac/removeAdmin
        [HttpPut("removeAdmin")]
        public async Task<IActionResult> RemoveAdmin([FromBody] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return NotFound(new { Message = "Bruger findes ikke." });
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
                if (result.Succeeded)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    return Ok(
                        new { Message = "Administratorrolle fjernet fra bruger", Roles = roles }
                    );
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }

            return Ok(new { Message = "Bruger har ikke administratorrolle" });
        }

        // if (user.Roles.Contains("Admin"))
        // {
        //     user.Roles.Remove("Admin"); // Fjerner admin rollen
        //     _context.Users.Update(user); // EF burde gøre dette automatisk
        //     await _context.SaveChangesAsync(); // Gem ændringerne i databasen
        //     return Ok(new { Message = "Removed Admin role", Roles = user.Roles });
        // }
        // return Ok(new { Message = "User doesn't have admin status" });
    }

    public class UserEmailDto
    {
        public string Email { get; set; } = string.Empty;
    }
}
