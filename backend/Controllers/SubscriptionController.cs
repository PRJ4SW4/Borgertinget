// backend.Controllers/SubscriptionController.cs
using backend.Data;
// using backend.DTOs; // Bruger kun SubscribeDto defineret nedenfor
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System; // Til Exception
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Base Route: /api/subscriptions
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly DataContext _context;

        public SubscriptionController(DataContext context)
        {
            _context = context;
        }

        // --- POST /api/subscriptions ---
        [HttpPost]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeDto subscribeDto)
        {
            // ... (din eksisterende Subscribe kode) ...
             var userIdString = User.FindFirstValue("userId");
             if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId)) { return Unauthorized("Kunne ikke identificere brugeren."); }
             int politicianTwitterId = subscribeDto.PoliticianId;
             var politicianExists = await _context.PoliticianTwitterIds.AnyAsync(p => p.Id == politicianTwitterId);
             if (!politicianExists) { return BadRequest($"Politiker med ID {politicianTwitterId} findes ikke."); }
             bool alreadySubscribed = await _context.Subscriptions.AnyAsync(s => s.UserId == currentUserId && s.PoliticianTwitterId == politicianTwitterId);
             if (alreadySubscribed) { return Conflict("Du abonnerer allerede på denne politiker."); }
             var newSubscription = new Subscription { UserId = currentUserId, PoliticianTwitterId = politicianTwitterId };
             try { _context.Subscriptions.Add(newSubscription); await _context.SaveChangesAsync(); return Ok("Abonnement oprettet."); }
             catch (DbUpdateException ex) { Console.WriteLine($"Fejl ved oprettelse af abonnement: {ex}"); return StatusCode(500, "Intern fejl ved oprettelse af abonnement."); }
        }

        // --- DELETE /api/subscriptions/{politicianTwitterId} ---
        [HttpDelete("{politicianTwitterId}")]
        public async Task<IActionResult> Unsubscribe(int politicianTwitterId)
        {
            // ... (din eksisterende Unsubscribe kode) ...
            var userIdString = User.FindFirstValue("userId");
             if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId)) { return Unauthorized("Kunne ikke identificere brugeren."); }
             var subscription = await _context.Subscriptions.FirstOrDefaultAsync(s => s.UserId == currentUserId && s.PoliticianTwitterId == politicianTwitterId);
             if (subscription == null) { return NotFound("Abonnement ikke fundet."); }
             try { _context.Subscriptions.Remove(subscription); await _context.SaveChangesAsync(); return Ok("Abonnement slettet."); }
             catch (DbUpdateException ex) { Console.WriteLine($"Fejl ved sletning af abonnement: {ex}"); return StatusCode(500, "Intern fejl ved sletning af abonnement."); }
        }
// Inde i SubscriptionController.cs

        [HttpGet("lookup/politicianTwitterId")]
        [Authorize] // Kræver login som du ønskede
        public async Task<ActionResult<object>> GetPoliticianTwitterIdByAktorId([FromQuery] int aktorId)
        {
            if (aktorId <= 0)
            {
                Console.WriteLine($"DEBUG Lookup: Received invalid Aktor ID = {aktorId}"); // Log invalid input
                return BadRequest("Ugyldigt Aktor ID.");
            }

            try
            {
                // --- DEBUG LOG 1 ---
                // Log hvilket AktorId vi leder efter
                Console.WriteLine($"DEBUG Lookup: Attempting to find PoliticianTwitterId for AktorId = {aktorId}");

                // Databasekaldet (uændret)
                var politicianInfo = await _context.PoliticianTwitterIds
                    .AsNoTracking()
                    .Where(p => p.AktorId == aktorId) // Finder match på AktorId
                    .Select(p => new { politicianTwitterId = p.Id }) // Vælger kun ID'et
                    .FirstOrDefaultAsync();

                // --- DEBUG LOG 2 ---
                // Log resultatet af databasekaldet
                Console.WriteLine($"DEBUG Lookup: Result from DB lookup (politicianInfo): {(politicianInfo == null ? "NULL" : $"Found ID {politicianInfo.politicianTwitterId}")}");

                if (politicianInfo == null)
                {
                    // --- DEBUG LOG 3 ---
                    // Log hvorfor vi returnerer 404
                    Console.WriteLine($"DEBUG Lookup: Returning 404 Not Found because politicianInfo was null.");
                    return NotFound($"Ingen tilknyttet 'PoliticianTwitterId' fundet for Aktor ID {aktorId}. Er data linket i databasen?");
                }

                // --- DEBUG LOG 4 ---
                // Log hvad vi sender tilbage ved succes (200 OK)
                Console.WriteLine($"DEBUG Lookup: Returning 200 OK with politicianTwitterId = {politicianInfo.politicianTwitterId}");
                return Ok(politicianInfo); // Returnerer { "politicianTwitterId": ID }
            }
            catch (Exception ex)
            {
                // Log eventuelle uventede fejl
                Console.WriteLine($"Fejl ved opslag af PoliticianTwitterId for AktorId {aktorId}: {ex}");
                return StatusCode(500, "Intern fejl ved opslag.");
            }
        }
    }
    public class SubscribeDto

{

 [Required]

public int PoliticianId { get; set; } // Forventer PoliticianTwitterId.Id her

}
    
    }