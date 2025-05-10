// backend.Controllers/SubscriptionController.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Data;
// using backend.DTOs; // Bruger kun SubscribeDto defineret nedenfor
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // Subscribe: Opretter et abonnement mellem den aktuelle bruger og en politiker
        //  Validerer at både bruger og politiker findes
        //  Tjekker at brugeren ikke allerede følger politikeren
        // tilføjer et nyt subscription i databasen
        [HttpPost]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeDto subscribeDto)
        {
            var userIdString = User.FindFirstValue("userId");
            if (
                string.IsNullOrEmpty(userIdString)
                || !int.TryParse(userIdString, out int currentUserId)
            )
            {
                return Unauthorized("Kunne ikke identificere brugeren.");
            }

            int politicianTwitterId = subscribeDto.PoliticianId;
            var politicianExists = await _context.PoliticianTwitterIds.AnyAsync(p =>
                p.Id == politicianTwitterId
            );

            if (!politicianExists)
            {
                return BadRequest($"Politiker med ID {politicianTwitterId} findes ikke.");
            }

            bool alreadySubscribed = await _context.Subscriptions.AnyAsync(s =>
                s.UserId == currentUserId && s.PoliticianTwitterId == politicianTwitterId
            );

            if (alreadySubscribed)
            {
                return Conflict("Du abonnerer allerede på denne politiker.");
            }

            var newSubscription = new Subscription
            {
                UserId = currentUserId,
                PoliticianTwitterId = politicianTwitterId,
            };

            try
            {
                _context.Subscriptions.Add(newSubscription);
                await _context.SaveChangesAsync();
                return Ok("Abonnement oprettet.");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Fejl ved oprettelse af abonnement: {ex}");
                return StatusCode(500, "Intern fejl ved oprettelse af abonnement.");
            }
        }

        // Unsubscribe: Fjerner et eksisterende abonnement mellem bruger og politiker
        // Finder det specifikke abonnement baseret på bruger-ID og politiker-ID
        // Sletter abonnementet fra databasen hvis det findes
        // Returnerer fejl hvis abonnementet ikke eksisterer
        [HttpDelete("{politicianTwitterId}")]
        public async Task<IActionResult> Unsubscribe(int politicianTwitterId)
        {
            var userIdString = User.FindFirstValue("userId");
            if (
                string.IsNullOrEmpty(userIdString)
                || !int.TryParse(userIdString, out int currentUserId)
            )
            {
                return Unauthorized("Kunne ikke identificere brugeren.");
            }

            var subscription = await _context.Subscriptions.FirstOrDefaultAsync(s =>
                s.UserId == currentUserId && s.PoliticianTwitterId == politicianTwitterId
            );

            if (subscription == null)
            {
                return NotFound("Abonnement ikke fundet.");
            }

            try
            {
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
                return Ok("Abonnement slettet.");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Fejl ved sletning af abonnement: {ex}");
                return StatusCode(500, "Intern fejl ved sletning af abonnement.");
            }
        }

        // Lookup: Oversætter mellem AktorId og PoliticianTwitterId
        // - Tager et AktorId
        // - Søger i databasen efter den tilsvarende politiker
        // - Returnerer tilhørende PoliticianTwitterId hvis fundet
        // - Bruges til integration mellem Folketingets data og applikationens politiker-data
        [HttpGet("lookup/politicianTwitterId")]
        [Authorize]
        public async Task<ActionResult<object>> GetPoliticianTwitterIdByAktorId(
            [FromQuery] int aktorId
        )
        {
            if (aktorId <= 0)
            {
                Console.WriteLine($"DEBUG Lookup: Received invalid Aktor ID = {aktorId}"); // Log invalid input
                return BadRequest("Ugyldigt Aktor ID.");
            }

            try
            {
                Console.WriteLine(
                    $"DEBUG Lookup: Attempting to find PoliticianTwitterId for AktorId = {aktorId}"
                );

                var politicianInfo = await _context
                    .PoliticianTwitterIds.AsNoTracking()
                    .Where(p => p.AktorId == aktorId) // Finder match på AktorId
                    .Select(p => new { politicianTwitterId = p.Id }) // Vælger kun ID'et
                    .FirstOrDefaultAsync();

                Console.WriteLine(
                    $"DEBUG Lookup: Result from DB lookup (politicianInfo): "
                        + $"{(politicianInfo == null ? "NULL" : $"Found ID {politicianInfo.politicianTwitterId}")}"
                );

                if (politicianInfo == null)
                {
                    Console.WriteLine(
                        $"DEBUG Lookup: Returning 404 Not Found because politicianInfo was null."
                    );
                    return NotFound(
                        $"Ingen tilknyttet 'PoliticianTwitterId' fundet for Aktor ID {aktorId}. "
                            + $"Er data linket i databasen?"
                    );
                }

                Console.WriteLine(
                    $"DEBUG Lookup: Returning 200 OK with politicianTwitterId = {politicianInfo.politicianTwitterId}"
                );
                return Ok(politicianInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Fejl ved opslag af PoliticianTwitterId for AktorId {aktorId}: {ex}"
                );
                return StatusCode(500, "Intern fejl ved opslag.");
            }
        }
    }
}
