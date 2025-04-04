using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

using backend.DTOs;

[ApiController]
[Route("api/[controller]")]
public class TwitterController : ControllerBase
{
    private readonly TwitterService _twitterService;

    public TwitterController(TwitterService twitterService)
    {
        _twitterService = twitterService;
    }

    [HttpGet("tweets/{userId}")]
    public async Task<ActionResult<List<TweetDto>>> GetTweets(string userId)
    {
        var tweetDtos = await _twitterService.GetStructuredTweets(userId);
        return Ok(tweetDtos);
    }
}

