using backend.Models;
using backend.Models.Calendar;
using backend.Repositories.Calendar;
using backend.Services.Calendar;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Tests.Services.Calendar;

[TestFixture]
public class CalendarServiceTests
{
    private ICalendarEventRepository _mockCalendarEventRepository;
    private ILogger<CalendarService> _mockLogger;
    private UserManager<User> _mockUserManager;
    private CalendarService _uut;

    [SetUp]
    public void Setup()
    {
        _mockCalendarEventRepository = Substitute.For<ICalendarEventRepository>();
        _mockLogger = Substitute.For<ILogger<CalendarService>>();
        // Mock UserManager
        var store = Substitute.For<IUserStore<User>>();
        _mockUserManager = Substitute.For<UserManager<User>>(
            store,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );

        _uut = new CalendarService(_mockCalendarEventRepository, _mockLogger, _mockUserManager);
    }

    [TearDown]
    public void TearDown()
    {
        _mockUserManager.Dispose();
    }

    [Test]
    public async Task GetAllEventsAsDTOAsync_ReturnsEmpty_WhenRepositoryReturnsNull()
    {
        // Arrange
        _mockCalendarEventRepository
            .GetAllEventsAsync()
            .Returns(Task.FromResult<IEnumerable<CalendarEvent>>(null!));

        // Act
        var result = await _uut.GetAllEventsAsDTOAsync(1);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAllEventsAsDTOAsync_ReturnsEmpty_WhenRepositoryReturnsEmptyList()
    {
        // Arrange
        _mockCalendarEventRepository
            .GetAllEventsAsync()
            .Returns(Task.FromResult(Enumerable.Empty<CalendarEvent>()));

        // Act
        var result = await _uut.GetAllEventsAsDTOAsync(1);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAllEventsAsDTOAsync_MapsToDTO_Correctly()
    {
        // Arrange
        var userId = 1;
        var events = new List<CalendarEvent>
        {
            new CalendarEvent
            {
                Id = 1,
                Title = "Event 1",
                StartDateTimeUtc = DateTime.UtcNow,
                Location = "Location 1",
                SourceUrl = "url1",
                InterestedUsers = new List<EventInterest> { new EventInterest { UserId = userId } },
            },
            new CalendarEvent
            {
                Id = 2,
                Title = "Event 2",
                StartDateTimeUtc = DateTime.UtcNow.AddDays(1),
                Location = "Location 2",
                SourceUrl = "url2",
                InterestedUsers = new List<EventInterest>(),
            },
        };
        _mockCalendarEventRepository
            .GetAllEventsAsync()
            .Returns(Task.FromResult<IEnumerable<CalendarEvent>>(events));

        // Act
        var result = (await _uut.GetAllEventsAsDTOAsync(userId)).ToList();

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));

        Assert.That(result[0].Id, Is.EqualTo(events[0].Id));
        Assert.That(result[0].Title, Is.EqualTo(events[0].Title));
        Assert.That(result[0].StartDateTimeUtc, Is.EqualTo(events[0].StartDateTimeUtc));
        Assert.That(result[0].Location, Is.EqualTo(events[0].Location));
        Assert.That(result[0].SourceUrl, Is.EqualTo(events[0].SourceUrl));
        Assert.That(result[0].InterestedCount, Is.EqualTo(1));
        Assert.That(result[0].IsCurrentUserInterested, Is.True);

        Assert.That(result[1].Id, Is.EqualTo(events[1].Id));
        Assert.That(result[1].Title, Is.EqualTo(events[1].Title));
        Assert.That(result[1].StartDateTimeUtc, Is.EqualTo(events[1].StartDateTimeUtc));
        Assert.That(result[1].Location, Is.EqualTo(events[1].Location));
        Assert.That(result[1].SourceUrl, Is.EqualTo(events[1].SourceUrl));
        Assert.That(result[1].InterestedCount, Is.EqualTo(0));
        Assert.That(result[1].IsCurrentUserInterested, Is.False);
    }

    [Test]
    public async Task GetAllEventsAsDTOAsync_MapsToDTO_Correctly_WhenInterestedUsersIsEmpty()
    {
        // Arrange
        var userId = 1;
        var events = new List<CalendarEvent>
        {
            new CalendarEvent
            {
                Id = 1,
                Title = "Event 1",
                StartDateTimeUtc = DateTime.UtcNow,
                Location = "Location 1",
                SourceUrl = "url1",
                InterestedUsers = new List<EventInterest>(),
            },
        };
        _mockCalendarEventRepository
            .GetAllEventsAsync()
            .Returns(Task.FromResult<IEnumerable<CalendarEvent>>(events));

        // Act
        var result = (await _uut.GetAllEventsAsDTOAsync(userId)).ToList();

        // Assert
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Id, Is.EqualTo(events[0].Id));
        Assert.That(result[0].InterestedCount, Is.EqualTo(0));
        Assert.That(result[0].IsCurrentUserInterested, Is.False);
    }
}
