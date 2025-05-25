using NUnit.Framework;
using NSubstitute;
using backend.Controllers;
using backend.Services.Calendar;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using backend.Services.Calendar.Scraping;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System; // For Exception

namespace backend.Tests.Controllers
{
    [TestFixture]
    public class CalendarControllerTests
    {
        private ICalendarService _calendarService;
        private ILogger<CalendarController> _logger;
        private CalendarController _controller;
        private IScraperService _scraperService; 

        [SetUp]
        public void Setup()
        {
            _scraperService = Substitute.For<IScraperService>(); 
            _calendarService = Substitute.For<ICalendarService>();
            _logger = Substitute.For<ILogger<CalendarController>>();
            _controller = new CalendarController(
                _scraperService, 
                _calendarService,
                _logger
            );

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "123") 
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        #region ToggleInterest Tests

        [Test]
        public async Task ToggleInterest_ValidEventIdAndUser_ReturnsOkResultWithInterestData()
        {
            // Arrange
            var eventId = 1;
            var userId = "123"; 
            var serviceResult = (IsInterested: true, InterestedCount: 5);

            _calendarService.ToggleInterestAsync(eventId, userId)
                .Returns(Task.FromResult<(bool IsInterested, int InterestedCount)?>(serviceResult));

            // Act
            var actionResult = await _controller.ToggleInterest(eventId);

            // Assert
            Assert.That(actionResult, Is.InstanceOf<OkObjectResult>());
            var okResult = actionResult as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            
            // Hent værdien fra OkObjectResult
            var resultValue = okResult?.Value;
            Assert.That(resultValue, Is.Not.Null);

            var resultType = resultValue?.GetType();
            var isInterestedProperty = resultType?.GetProperty("isInterested");
            var interestedCountProperty = resultType?.GetProperty("interestedCount");

            Assert.That(isInterestedProperty, Is.Not.Null, "Property 'isInterested' blev ikke fundet på det returnerede objekt.");
            Assert.That(interestedCountProperty, Is.Not.Null, "Property 'interestedCount' blev ikke fundet på det returnerede objekt.");

            var actualIsInterested = (bool?)isInterestedProperty?.GetValue(resultValue);
            var actualInterestedCount = (int?)interestedCountProperty?.GetValue(resultValue);

            Assert.That(actualIsInterested, Is.EqualTo(serviceResult.IsInterested));
            Assert.That(actualInterestedCount, Is.EqualTo(serviceResult.InterestedCount));
        }

        [Test]
        public async Task ToggleInterest_ServiceReturnsNull_ReturnsNotFound()
        {
            // Arrange
            var eventId = 99; 
            var userId = "123";

            _calendarService.ToggleInterestAsync(eventId, userId)
                .Returns(Task.FromResult<(bool IsInterested, int InterestedCount)?>(null));

            // Act
            var actionResult = await _controller.ToggleInterest(eventId);

            // Assert
            Assert.That(actionResult, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = actionResult as NotFoundObjectResult;
            Assert.That(notFoundResult?.Value, Is.EqualTo("Event not found."));
        }

        [Test]
        public async Task ToggleInterest_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var eventId = 1;
            var userId = "123";

            _calendarService.ToggleInterestAsync(eventId, userId)
                .Returns(Task.FromException<(bool IsInterested, int InterestedCount)?>(new Exception("Test service exception")));

            // Act
            var actionResult = await _controller.ToggleInterest(eventId);

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ObjectResult>());
            var objectResult = actionResult as ObjectResult;
            Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult?.Value, Is.EqualTo("An internal error occurred while toggling interest."));
        }

        #endregion

        #region GetAmountInterested Tests

        [Test]
        public async Task GetAmountInterested_ValidEventId_ReturnsOkWithCount()
        {
            // Arrange
            var eventId = 1;
            var expectedCount = 10;

            _calendarService.GetAmountInterestedAsync(eventId)
                .Returns(Task.FromResult(expectedCount));

            // Act
            var actionResult = await _controller.GetAmountInterested(eventId);

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult?.Value, Is.EqualTo(expectedCount));
        }

        [Test]
        public async Task GetAmountInterested_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var eventId = 1;

            _calendarService.GetAmountInterestedAsync(eventId)
                .Returns(Task.FromException<int>(new Exception("Test service exception")));

            // Act
            var actionResult = await _controller.GetAmountInterested(eventId);

            // Assert
            Assert.That(actionResult.Result, Is.InstanceOf<ObjectResult>());
            var objectResult = actionResult.Result as ObjectResult;
            Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult?.Value, Is.EqualTo("An internal error occurred while fetching the number of interested users."));
        }

        #endregion
    }
}