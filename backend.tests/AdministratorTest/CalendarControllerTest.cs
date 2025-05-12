using System;
using System.Linq;
using System.Threading.Tasks;
using backend.Controllers;
using backend.Data;
using backend.DTO.Calendar;
using backend.Models.Calendar;
using backend.Services.AutomationServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Controllers
{
    [TestFixture]
    public class CalendarControllerAdminTests
    {
        private DataContext _context;
        private CalendarController _uut;
        private IAltingetScraperService _mockScraperService; // Changed to interface for NSubstitute
        private ILogger<CalendarController> _mockLogger;

        [SetUp]
        public void Setup()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use unique name to ensure isolation
                .Options;
            _context = new DataContext(options);
            _context.Database.EnsureCreated();

            _mockScraperService = Substitute.For<IAltingetScraperService>();
            _mockLogger = Substitute.For<ILogger<CalendarController>>();

            _uut = new CalendarController(_mockScraperService, _context, _mockLogger);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task CreateEvent_ValidEvent_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var newEventDto = new CalendarEventDTO
            {
                Title = "Test Event",
                StartDateTimeUtc = DateTimeOffset.UtcNow.AddDays(1),
                Location = "Test Location",
                SourceUrl = "/test-event-url",
            };

            // Act
            var result = await _uut.CreateEvent(newEventDto);

            // Assert
            Assert.That(result, Is.TypeOf<ActionResult<CalendarEventDTO>>());
            var actionResult = result.Result as CreatedAtActionResult;
            Assert.That(actionResult, Is.Not.Null);
            Assert.That(actionResult.StatusCode, Is.EqualTo(201));
            var createdEventDto = actionResult.Value as CalendarEventDTO;
            Assert.That(createdEventDto, Is.Not.Null);
            Assert.That(createdEventDto.Title, Is.EqualTo(newEventDto.Title));
            Assert.That(createdEventDto.SourceUrl, Is.EqualTo(newEventDto.SourceUrl));

            var dbEvent = await _context.CalendarEvents.FindAsync(createdEventDto.Id);
            Assert.That(dbEvent, Is.Not.Null);
            Assert.That(dbEvent.Title, Is.EqualTo(newEventDto.Title));
        }

        [Test]
        public async Task CreateEvent_MissingSourceUrl_ReturnsBadRequest()
        {
            // Arrange
            var newEventDto = new CalendarEventDTO
            {
                Title = "Test Event No Source",
                StartDateTimeUtc = DateTimeOffset.UtcNow.AddDays(1),
                Location = "Test Location",
                SourceUrl = null, // Missing SourceUrl
            };

            // Act
            var result = await _uut.CreateEvent(newEventDto);

            // Assert
            Assert.That(result, Is.TypeOf<ActionResult<CalendarEventDTO>>());
            var badRequestResult = result.Result as BadRequestObjectResult; // Controller returns BadRequestObjectResult
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
            Assert.That(badRequestResult.Value, Is.EqualTo("SourceUrl is required."));
        }

        [Test]
        public async Task UpdateEvent_ValidEvent_ReturnsNoContentResult()
        {
            // Arrange
            var initialEvent = new CalendarEvent
            {
                Title = "Initial Event",
                StartDateTimeUtc = DateTimeOffset.UtcNow,
                Location = "Initial Location",
                SourceUrl = "/initial-url",
                LastScrapedUtc = DateTimeOffset.UtcNow,
            };
            _context.CalendarEvents.Add(initialEvent);
            await _context.SaveChangesAsync();
            // Detach to avoid tracking issues if the context is reused or entities are modified outside of EF's tracking
            _context.Entry(initialEvent).State = EntityState.Detached;

            var updatedEventDto = new CalendarEventDTO
            {
                Id = initialEvent.Id,
                Title = "Updated Event Title",
                StartDateTimeUtc = DateTimeOffset.UtcNow.AddHours(1),
                Location = "Updated Location",
                SourceUrl = "/updated-url",
            };

            // Act
            var result = await _uut.UpdateEvent(initialEvent.Id, updatedEventDto);

            // Assert
            Assert.That(result, Is.TypeOf<NoContentResult>());
            var updatedDbEvent = await _context.CalendarEvents.FindAsync(initialEvent.Id);
            Assert.That(updatedDbEvent, Is.Not.Null);
            Assert.That(updatedDbEvent.Title, Is.EqualTo(updatedEventDto.Title));
            Assert.That(updatedDbEvent.Location, Is.EqualTo(updatedEventDto.Location));
            Assert.That(updatedDbEvent.SourceUrl, Is.EqualTo(updatedEventDto.SourceUrl));
        }

        [Test]
        public async Task UpdateEvent_MismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var eventDto = new CalendarEventDTO
            {
                Id = 1,
                Title = "Test",
                SourceUrl = "/test",
            };

            // Act
            var result = await _uut.UpdateEvent(2, eventDto); // URL ID is 2, DTO ID is 1

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
            Assert.That(badRequestResult.Value, Is.EqualTo("ID in URL and body must match."));
        }

        [Test]
        public async Task UpdateEvent_MissingSourceUrl_ReturnsBadRequest()
        {
            // Arrange
            var eventDto = new CalendarEventDTO
            {
                Id = 1,
                Title = "Test",
                SourceUrl = "",
            }; // Empty SourceUrl

            // Act
            var result = await _uut.UpdateEvent(1, eventDto);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
            Assert.That(badRequestResult.Value, Is.EqualTo("SourceUrl is required."));
        }

        [Test]
        public async Task UpdateEvent_EventNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var nonExistentEventId = 999;
            var eventDto = new CalendarEventDTO
            {
                Id = nonExistentEventId,
                Title = "Non Existent",
                SourceUrl = "/non-existent",
            };

            // Act
            var result = await _uut.UpdateEvent(nonExistentEventId, eventDto);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteEvent_ExistingEvent_ReturnsNoContentResult()
        {
            // Arrange
            var eventToDelete = new CalendarEvent
            {
                Title = "Event to Delete",
                StartDateTimeUtc = DateTimeOffset.UtcNow,
                Location = "Delete Location",
                SourceUrl = "/delete-url",
                LastScrapedUtc = DateTimeOffset.UtcNow,
            };
            _context.CalendarEvents.Add(eventToDelete);
            await _context.SaveChangesAsync();
            var eventId = eventToDelete.Id;
            _context.Entry(eventToDelete).State = EntityState.Detached;

            // Act
            var result = await _uut.DeleteEvent(eventId);

            // Assert
            Assert.That(result, Is.TypeOf<NoContentResult>());
            var deletedEvent = await _context.CalendarEvents.FindAsync(eventId);
            Assert.That(deletedEvent, Is.Null);
        }

        [Test]
        public async Task DeleteEvent_NonExistingEvent_ReturnsNotFoundResult()
        {
            // Arrange
            var nonExistentEventId = 999;

            // Act
            var result = await _uut.DeleteEvent(nonExistentEventId);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }
    }
}
