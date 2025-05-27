using backend.DTO.Calendar;
using backend.Models;
using backend.Models.Calendar;
using backend.Repositories.Calendar;
using backend.Services.Calendar;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Services;

[TestFixture]
public class CalendarServiceTest
{
    private CalendarService _uut;
    private ICalendarEventRepository _repository;
    private ILogger<CalendarService> _logger;

    private UserManager<User> _mockUserManager;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<ICalendarEventRepository>();
        _logger = Substitute.For<ILogger<CalendarService>>();
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
        _uut = new CalendarService(_repository, _logger, _mockUserManager);
    }

    [TearDown]
    public void TearDown()
    {
        _mockUserManager.Dispose();
    }

    #region CreateEventAsync Tests

    [Test]
    public async Task CreateEventAsync_ShouldReturnDTO_WhenCreationIsSuccessful()
    {
        // Arrange
        var eventDto = new CalendarEventDTO { Id = 0, Title = "New Event" };
        var calendarEvent = new CalendarEvent { Id = 0, Title = eventDto.Title };

        _repository.AddEventAsync(calendarEvent).Returns(Task.CompletedTask);
        _repository.SaveChangesAsync().Returns(1).AndDoes(_ => calendarEvent.Id = eventDto.Id);

        // Act
        var result = await _uut.CreateEventAsync(eventDto);

        // Assert
        Assert.That(result.Id, Is.EqualTo(eventDto.Id));
        Assert.That(result.Title, Is.EqualTo(eventDto.Title));
        Assert.That(result.StartDateTimeUtc, Is.EqualTo(eventDto.StartDateTimeUtc));
        Assert.That(result.Location, Is.EqualTo(eventDto.Location));
        Assert.That(result.SourceUrl, Is.EqualTo(string.Empty));
    }

    #endregion

    #region UpdateEventAsync Tests

    [Test]
    public async Task UpdateEventAsync_ShouldReturnTrue_WhenUpdateIsSuccessful()
    {
        // Arrange
        var eventId = 1;
        var eventDto = new CalendarEventDTO { Id = eventId, Title = "Updated Event" };
        var calendarEvent = new CalendarEvent { Id = eventId };

        _repository.GetEventByIdAsync(eventId).Returns(calendarEvent);
        _repository.SaveChangesAsync().Returns(1);

        // Act
        var result = await _uut.UpdateEventAsync(eventId, eventDto);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(calendarEvent.Title, Is.EqualTo(eventDto.Title));
    }

    [Test]
    public async Task UpdateEventAsync_ShouldReturnFalse_WhenEventNotFound()
    {
        // Arrange
        var eventId = 1;
        var eventDto = new CalendarEventDTO { Id = eventId, Title = "Updated Event" };

        _repository.GetEventByIdAsync(eventId).Returns((CalendarEvent?)null);

        // Act
        var result = await _uut.UpdateEventAsync(eventId, eventDto);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion

    #region DeleteEventAsync Tests

    [Test]
    public async Task DeleteEventAsync_ShouldReturnTrue_WhenDeletionIsSuccessful()
    {
        // Arrange
        var eventId = 1;
        var calendarEvent = new CalendarEvent { Id = eventId };

        _repository.GetEventByIdAsync(eventId).Returns(calendarEvent);
        _repository.SaveChangesAsync().Returns(1);

        // Act
        var result = await _uut.DeleteEventAsync(eventId);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DeleteEventAsync_ShouldReturnFalse_WhenEventNotFound()
    {
        // Arrange
        var eventId = 1;

        _repository.GetEventByIdAsync(eventId).Returns((CalendarEvent?)null);

        // Act
        var result = await _uut.DeleteEventAsync(eventId);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion
}
