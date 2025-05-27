using System.Security.Claims;
using backend.Controllers;
using backend.DTO.Calendar;
using backend.Services.Calendar;
using backend.Services.Calendar.Scraping;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Tests.Controllers;

[TestFixture]
public class CalendarControllerTests
{
    private CalendarController _uut;
    private IScraperService _mockScraperService;
    private ICalendarService _mockCalendarService;
    private ILogger<CalendarController> _mockLogger;

    [SetUp]
    public void Setup()
    {
        _mockScraperService = Substitute.For<IScraperService>();
        _mockCalendarService = Substitute.For<ICalendarService>();
        _mockLogger = Substitute.For<ILogger<CalendarController>>();

        _uut = new CalendarController(_mockScraperService, _mockCalendarService, _mockLogger);
    }

    [TearDown]
    public void TearDown() { }

    [Test]
    public async Task GetEvents_ReturnsOkResult_WithEvents()
    {
        // Arrange
        var expectedEvents = new List<CalendarEventDTO> { new CalendarEventDTO() };
        _mockCalendarService
            .GetAllEventsAsDTOAsync(Arg.Any<int>())
            .Returns(Task.FromResult<IEnumerable<CalendarEventDTO>>(expectedEvents));

        var user = new ClaimsPrincipal(
            new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "mock")
        );
        _uut.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user },
        };

        // Act
        var result = await _uut.GetEvents();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(expectedEvents));
    }

    [Test]
    public async Task GetEvents_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        _mockCalendarService
            .GetAllEventsAsDTOAsync(Arg.Any<int>())
            .ThrowsAsync(new Exception("Service error"));
        var user = new ClaimsPrincipal(
            new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "mock")
        );
        _uut.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user },
        };

        // Act
        var result = await _uut.GetEvents();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
        Assert.That(
            objectResult.Value,
            Is.EqualTo("An internal error occurred while fetching events.")
        );
    }

    [Test]
    public async Task RunScraperEndpoint_ReturnsOkResult_WhenScraperSucceeds()
    {
        // Arrange
        _mockScraperService.RunScraper().Returns(Task.FromResult(5));

        // Act
        var result = await _uut.RunScraperEndpoint();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo("Scrape automation ran. Found/Processed 5 events."));
    }

    [Test]
    public async Task RunScraperEndpoint_ReturnsStatusCode500_WhenScraperFails()
    {
        // Arrange
        _mockScraperService.RunScraper().Returns(Task.FromResult(-1));

        // Act
        var result = await _uut.RunScraperEndpoint();

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult.Value, Is.EqualTo("Scrape automation failed."));
    }

    [Test]
    public async Task RunScraperEndpoint_ReturnsStatusCode500_WhenScraperThrowsException()
    {
        // Arrange
        _mockScraperService.RunScraper().ThrowsAsync(new Exception("Scraper error"));

        // Act
        var result = await _uut.RunScraperEndpoint();

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult.StatusCode, Is.EqualTo(500));
        Assert.That(
            objectResult.Value,
            Is.EqualTo("An error occurred while running scrape automation.")
        );
    }
}
