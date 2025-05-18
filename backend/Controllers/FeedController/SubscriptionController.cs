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
            ILogger<SubscriptionController> logger)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeDto subscribeDto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            {
                return Unauthorized("Kunne ikke identificere brugeren.");
            }

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
        public async Task<IActionResult> Unsubscribe(int politicianTwitterId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int currentUserId))
            {
                return Unauthorized("Kunne ikke identificere brugeren.");
            }

            try
            {
                var (success, message) = await _subscriptionService.UnsubscribeAsync(currentUserId, politicianTwitterId);
                
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
        public async Task<ActionResult<object>> GetPoliticianTwitterIdByAktorId([FromQuery] int aktorId)
        {
            if (aktorId <= 0)
            {
                _logger.LogWarning("Received invalid Aktor ID = {AktorId}", aktorId);
                return BadRequest("Ugyldigt Aktor ID.");
            }

            try
            {
                var (success, result, message) = await _subscriptionService.LookupPoliticianAsync(aktorId);

                if (!success)
                    return NotFound(message);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fejl ved opslag af PoliticianTwitterId for AktorId {AktorId}", aktorId);
                return StatusCode(500, "Intern fejl ved opslag.");
            }
        }
    }
}
