using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Services.Feed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [ApiController]
    [Route("api")]
    public class FeedController : ControllerBase
    {
        private readonly IFeedService _feedService;
        private readonly ILogger<FeedController> _logger;

        public FeedController(IFeedService feedService, ILogger<FeedController> logger)
        {
            _feedService = feedService ?? throw new ArgumentNullException(nameof(feedService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GetMySubscriptions: Henter brugerens abonnementer på politikere
        // først findes brugeren user id fra jwt fra token
        // Returnerer en liste af politikere som brugeren følger
        [Authorize]
        [HttpGet("subscriptions")]
        public async Task<ActionResult<List<PoliticianInfoDto>>> GetMySubscriptions()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int currentUserId);

            try
            {
                var subscriptions = await _feedService.GetUserSubscriptionsAsync(currentUserId);
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Fejl under hentning af subscriptions for bruger {currentUserId}"
                );
                return StatusCode(500, "Intern fejl ved hentning af abonnementer.");
            }
        }

        //  GetMyFeed dette er så selve "cobtroller" til at hente tweets og polls
        [Authorize]
        [HttpGet("feed")]
        // Returnerer den PaginatedFeedResult DTO, som nu har Tweets og LatestPolls
        public async Task<ActionResult<PaginatedFeedResult>> GetMyFeed(
            // Paginering: page og pageSize standardværdier, vi starter på page 1 og 5 tweets pr. side
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] int? politicianId = null
        )
        {
            // først findes brugeren user id fra jwt fra token, samme stil, som før i "GetMySubscriptions" endpointet
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out int currentUserId);

            try
            {
                var result = await _feedService.GetUserFeedAsync(
                    currentUserId,
                    page,
                    pageSize,
                    politicianId
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Fejl under hentning af feed for bruger {currentUserId} (Filter: {politicianId})"
                );
                return StatusCode(500, "Der opstod en intern fejl under hentning af dit feed.");
            }
        }
    }
}
