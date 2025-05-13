using System;
using System.Linq;
using System.Threading.Tasks;
using backend.Controllers;
using backend.Data;
using backend.DTO.Calendar;
using backend.Models.Calendar;
using backend.Services.Calendar;
using backend.Services.Calendar.Scraping;
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
        private IScraperService _mockScraperService;
        private ICalendarService _mockCalendarService;
        private ILogger<CalendarController> _mockLogger;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new DataContext(options);
            _context.Database.EnsureCreated();

            _mockScraperService = Substitute.For<IScraperService>();
            _mockCalendarService = Substitute.For<ICalendarService>();
            _mockLogger = Substitute.For<ILogger<CalendarController>>();

            _uut = new CalendarController(_mockScraperService, _mockCalendarService, _mockLogger);
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
            var newEventDto = new CalendarEventDTO
            {
                Title = "Test Event",
                StartDateTimeUtc = DateTimeOffset.UtcNow.AddDays(1),
                Location = "Test Location",
                SourceUrl = "/test-event-url",
            };
            var createdEventDtoFromService = new CalendarEventDTO
            {
                Id = 1,
                Title = newEventDto.Title,
                StartDateTimeUtc = newEventDto.StartDateTimeUtc,
                Location = newEventDto.Location,
                SourceUrl = newEventDto.SourceUrl,
            };

            _mockCalendarService
                .CreateEventAsync(newEventDto)
                .Returns(Task.FromResult(createdEventDtoFromService));

            var result = await _uut.CreateEvent(newEventDto);

            Assert.That(result, Is.TypeOf<ActionResult<CalendarEventDTO>>());
            var actionResult = result.Result as CreatedAtActionResult;
            Assert.That(actionResult, Is.Not.Null);
            Assert.That(actionResult.StatusCode, Is.EqualTo(201));
            var returnedDto = actionResult.Value as CalendarEventDTO;
            Assert.That(returnedDto, Is.Not.Null);
            Assert.That(returnedDto.Id, Is.EqualTo(createdEventDtoFromService.Id));
            Assert.That(returnedDto.Title, Is.EqualTo(newEventDto.Title));
            Assert.That(returnedDto.SourceUrl, Is.EqualTo(newEventDto.SourceUrl));

            await _mockCalendarService.Received(1).CreateEventAsync(newEventDto);
        }

        [Test]
        public async Task CreateEvent_MissingSourceUrl_ReturnsBadRequest()
        {
            var newEventDto = new CalendarEventDTO
            {
                Title = "Test Event No Source",
                StartDateTimeUtc = DateTimeOffset.UtcNow.AddDays(1),
                Location = "Test Location",
                SourceUrl = null,
            };

            var result = await _uut.CreateEvent(newEventDto);

            Assert.That(result, Is.TypeOf<ActionResult<CalendarEventDTO>>());
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));
            Assert.That(badRequestResult.Value, Is.EqualTo("SourceUrl is required."));

            await _mockCalendarService
                .DidNotReceive()
                .CreateEventAsync(Arg.Any<CalendarEventDTO>());
        }

        [Test]
        public async Task UpdateEvent_ValidEvent_ReturnsNoContentResult()
        {
            var eventId = 1;
            var updatedEventDto = new CalendarEventDTO
            {
                Id = eventId,
                Title = "Updated Event Title",
                StartDateTimeUtc = DateTimeOffset.UtcNow.AddHours(1),
                Location = "Updated Location",
                SourceUrl = "/updated-url",
            };

            _mockCalendarService
                .UpdateEventAsync(eventId, updatedEventDto)
                .Returns(Task.FromResult(true));

            var result = await _uut.UpdateEvent(eventId, updatedEventDto);

            Assert.That(result, Is.TypeOf<NoContentResult>());
            await _mockCalendarService.Received(1).UpdateEventAsync(eventId, updatedEventDto);
        }

        [Test]
        public async Task UpdateEvent_MismatchedId_ReturnsBadRequest()
        {
            var eventDto = new CalendarEventDTO
            {
                Id = 1,
                Title = "Test",
                SourceUrl = "/test",
            };

            var result = await _uut.UpdateEvent(2, eventDto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult?.StatusCode, Is.EqualTo(400));
            Assert.That(badRequestResult?.Value, Is.EqualTo("ID in URL and body must match."));
            await _mockCalendarService
                .DidNotReceive()
                .UpdateEventAsync(Arg.Any<int>(), Arg.Any<CalendarEventDTO>());
        }

        [Test]
        public async Task UpdateEvent_MissingSourceUrl_ReturnsBadRequest()
        {
            var eventId = 1;
            var eventDto = new CalendarEventDTO
            {
                Id = eventId,
                Title = "Test",
                SourceUrl = "",
            };

            var result = await _uut.UpdateEvent(eventId, eventDto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult?.StatusCode, Is.EqualTo(400));
            Assert.That(badRequestResult?.Value, Is.EqualTo("SourceUrl is required."));
            await _mockCalendarService
                .DidNotReceive()
                .UpdateEventAsync(Arg.Any<int>(), Arg.Any<CalendarEventDTO>());
        }

        [Test]
        public async Task UpdateEvent_EventNotFound_ReturnsNotFoundResult()
        {
            var nonExistentEventId = 999;
            var eventDto = new CalendarEventDTO
            {
                Id = nonExistentEventId,
                Title = "Non Existent",
                SourceUrl = "/non-existent",
            };

            _mockCalendarService
                .UpdateEventAsync(nonExistentEventId, eventDto)
                .Returns(Task.FromResult(false));

            var result = await _uut.UpdateEvent(nonExistentEventId, eventDto);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
            await _mockCalendarService.Received(1).UpdateEventAsync(nonExistentEventId, eventDto);
        }

        [Test]
        public async Task DeleteEvent_ExistingEvent_ReturnsNoContentResult()
        {
            var eventId = 1;
            _mockCalendarService.DeleteEventAsync(eventId).Returns(Task.FromResult(true));

            var result = await _uut.DeleteEvent(eventId);

            Assert.That(result, Is.TypeOf<NoContentResult>());
            await _mockCalendarService.Received(1).DeleteEventAsync(eventId);
        }

        [Test]
        public async Task DeleteEvent_NonExistingEvent_ReturnsNotFoundResult()
        {
            var nonExistentEventId = 999;
            _mockCalendarService
                .DeleteEventAsync(nonExistentEventId)
                .Returns(Task.FromResult(false));

            var result = await _uut.DeleteEvent(nonExistentEventId);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
            await _mockCalendarService.Received(1).DeleteEventAsync(nonExistentEventId);
        }
    }
}
