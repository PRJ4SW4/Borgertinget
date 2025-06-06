using System.Security.Claims;
using backend.Controllers;
using backend.DTOs;
using backend.Services.Feed;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Tests.Controllers
{
    [TestFixture]
    public class FeedControllerTests
    {
        private FeedController _controller;
        private IFeedService _mockFeedService;
        private ILogger<FeedController> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockFeedService = Substitute.For<IFeedService>();
            _mockLogger = Substitute.For<ILogger<FeedController>>();
            _controller = new FeedController(_mockFeedService, _mockLogger);

            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "123") })
            );
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user },
            };
        }

        [Test]
        public async Task GetMySubscriptions_ValidUser_ReturnsOkWithSubscriptions()
        {
            var subscriptions = new List<PoliticianInfoDto>
            {
                new PoliticianInfoDto { Id = 1, Name = "Politician 1" },
            };
            _mockFeedService.GetUserSubscriptionsAsync(123).Returns(subscriptions);

            var result = await _controller.GetMySubscriptions();

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(subscriptions));
        }

        [Test]
        public async Task GetMySubscriptions_ServiceThrowsException_Returns500()
        {
            _mockFeedService
                .When(x => x.GetUserSubscriptionsAsync(123))
                .Do(x =>
                {
                    throw new Exception("Test exception");
                });

            var result = await _controller.GetMySubscriptions();

            Assert.That(result.Result, Is.TypeOf<ObjectResult>());
            var objectResult = result.Result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }

        [Test]
        public async Task GetMyFeed_ValidParameters_ReturnsOkWithFeed()
        {
            var feedResult = new PaginatedFeedResult
            {
                Tweets = new List<TweetDto>(),
                HasMore = false,
                LatestPolls = new List<PollDetailsDto>(),
            };
            _mockFeedService.GetUserFeedAsync(123, 1, 5, null).Returns(feedResult);

            var result = await _controller.GetMyFeed(1, 5, null);

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(feedResult));
        }

        [Test]
        public async Task GetMyFeed_WithFilter_ReturnsOkWithFilteredFeed()
        {
            int politicianId = 42;
            var feedResult = new PaginatedFeedResult();
            _mockFeedService.GetUserFeedAsync(123, 1, 5, politicianId).Returns(feedResult);

            var result = await _controller.GetMyFeed(1, 5, politicianId);

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMyFeed_ServiceThrowsException_Returns500()
        {
            _mockFeedService
                .When(x => x.GetUserFeedAsync(123, 1, 5, null))
                .Do(x =>
                {
                    throw new Exception("Test exception");
                });

            var result = await _controller.GetMyFeed();

            Assert.That(result.Result, Is.TypeOf<ObjectResult>());
            var objectResult = result.Result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(500));
        }
    }
}
