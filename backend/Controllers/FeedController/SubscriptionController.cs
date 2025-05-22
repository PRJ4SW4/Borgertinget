// backend.Controllers/SubscriptionController.cs
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Services.Subscription;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<SubscriptionController> _logger;

        public SubscriptionController(
            ISubscriptionService subscriptionService,
            ILogger<SubscriptionController> logger
        )
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeDto subscribeDto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int currentUserId);

            try
            {
                var (success, message) = await _subscriptionService.SubscribeAsync(
                    currentUserId,
                    subscribeDto.PoliticianId
                );

                if (!success)
                {
                    if (message.Contains("findes ikke"))
                        return BadRequest(message);
                    else if (message.Contains("allerede"))
                        return Conflict(message);
                    else
                        return StatusCode(500, message);
                }

                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved oprettelse af abonnement");
                return StatusCode(500, "Intern fejl ved oprettelse af abonnement.");
            }
        }

        [HttpDelete("{politicianTwitterId}")]
        [Authorize]
        public async Task<IActionResult> Unsubscribe(int politicianTwitterId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int currentUserId);

            try
            {
                var (success, message) = await _subscriptionService.UnsubscribeAsync(
                    currentUserId,
                    politicianTwitterId
                );

                if (!success)
                    return NotFound(message);

                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved sletning af abonnement");
                return StatusCode(500, "Intern fejl ved sletning af abonnement.");
            }
        }

        [HttpGet("lookup/politicianTwitterId")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PoliticianInfoDto>> GetPoliticianTwitterIdByAktorId(
            [FromQuery] int aktorId
        )
        {
            if (aktorId <= 0)
            {
                _logger.LogWarning("Received invalid Aktor ID = {AktorId}", aktorId);
                return BadRequest("Ugyldigt Aktor ID.");
            }

            try
            {
                var result = await _subscriptionService.LookupPoliticianAsync(aktorId);
                if (result == null)
                {
                    return NotFound("Politiker ikke fundet.");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Fejl ved opslag af PoliticianTwitterId for AktorId {AktorId}",
                    aktorId
                );
                return StatusCode(500, "Intern fejl ved opslag.");
            }
        }
    }
}
