using backend.DTO.Calendar;
using backend.Models.Calendar;
using backend.Repositories.Calendar;
using backend.Services.Calendar;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Services;

[TestFixture]
public class CalendarServiceTest
{
    private CalendarService _service;
    private ICalendarEventRepository _repository;
    private ILogger<CalendarService> _logger;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<ICalendarEventRepository>();
        _logger = Substitute.For<ILogger<CalendarService>>();
        _service = new CalendarService(_repository, _logger, null);
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
        var result = await _service.CreateEventAsync(eventDto);

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
        var result = await _service.UpdateEventAsync(eventId, eventDto);

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
        var result = await _service.UpdateEventAsync(eventId, eventDto);

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
        var result = await _service.DeleteEventAsync(eventId);

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
        var result = await _service.DeleteEventAsync(eventId);

        // Assert
        Assert.That(result, Is.False);
    }

    #endregion
}
