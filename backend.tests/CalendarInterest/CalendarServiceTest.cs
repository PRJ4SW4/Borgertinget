using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Models;
using backend.Models.Calendar;
using backend.Repositories.Calendar;
using backend.Services.Calendar;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace backend.Tests.Services
{
    [TestFixture]
    public class CalendarServiceTests
    {
        private ICalendarEventRepository _calendarEventRepository;
        private ILogger<CalendarService> _logger;
        private UserManager<User> _userManager;
        private CalendarService _service;

#pragma warning disable NUnit1032 //fordi den brokkede sig for meget over at denne ikke blev disposed, selvom den gjorde
        private IUserStore<User> _mockUserStore;
#pragma warning restore NUnit1032

        [SetUp]
        public void Setup()
        {
            _calendarEventRepository = Substitute.For<ICalendarEventRepository>();
            _logger = Substitute.For<ILogger<CalendarService>>();

            _mockUserStore = Substitute.For<IUserStore<User>>();
            _userManager = Substitute.For<UserManager<User>>(
                _mockUserStore,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null
            );

            _service = new CalendarService(_calendarEventRepository, _logger, _userManager);
        }

        [TearDown]
        public void Teardown()
        {
            _userManager?.Dispose();
            var disposableUserStore = _mockUserStore as IDisposable;
            disposableUserStore?.Dispose();
        }

        // hjælpemetode til warning handling
        private bool LogMessageContains(object? logState, string expectedMessagePart)
        {
            if (logState == null)
            {
                return false;
            }

            return logState.ToString()?.Contains(expectedMessagePart) ?? false;
        }

        #region ToggleInterestAsync Tests

        [Test]
        public async Task ToggleInterestAsync_UserAndEventExist_NotAlreadyInterested_AddsInterest_ReturnsCorrectData()
        {
            // Arrange
            var eventId = 1;
            var userIdString = "123";
            var parsedUserId = 123;
            var user = new User { Id = parsedUserId, UserName = "TestUser" };
            var calendarEvent = new CalendarEvent
            {
                Id = eventId,
                InterestedUsers = new List<EventInterest>(),
            };
            var expectedCountAfterAdd = 1;

            _userManager.FindByIdAsync(userIdString).Returns(Task.FromResult<User?>(user));
            _calendarEventRepository
                .GetEventByIdAsync(eventId)
                .Returns(Task.FromResult<CalendarEvent?>(calendarEvent));
            _calendarEventRepository
                .RetrieveInterestPairsAsync(eventId, parsedUserId)
                .Returns(Task.FromResult<EventInterest?>(null)); 
            _calendarEventRepository
                .GetInterestedUsersAsync(eventId)
                .Returns(Task.FromResult(expectedCountAfterAdd));
            _calendarEventRepository.SaveChangesAsync().Returns(Task.FromResult(1)); 

            // Act
            var result = await _service.ToggleInterestAsync(eventId, userIdString);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value.IsInterested, Is.True);
            Assert.That(result.Value.InterestedCount, Is.EqualTo(expectedCountAfterAdd));
            await _calendarEventRepository
                .Received(1)
                .AddEventInterest(
                    Arg.Is<EventInterest>(ei =>
                        ei.CalendarEventId == eventId && ei.UserId == parsedUserId
                    )
                );
            await _calendarEventRepository.Received(1).SaveChangesAsync();
        }

        [Test]
        public async Task ToggleInterestAsync_UserAndEventExist_AlreadyInterested_RemovesInterest_ReturnsCorrectData()
        {
            // Arrange
            var eventId = 1;
            var userIdString = "123";
            var parsedUserId = 123;
            var user = new User { Id = parsedUserId, UserName = "TestUser" };
            var existingInterest = new EventInterest
            {
                CalendarEventId = eventId,
                UserId = parsedUserId,
                EventInterestId = 1,
            };
            var calendarEvent = new CalendarEvent
            {
                Id = eventId,
                InterestedUsers = new List<EventInterest> { existingInterest },
            };
            var expectedCountAfterRemove = 0;

            _userManager.FindByIdAsync(userIdString).Returns(Task.FromResult<User?>(user));
            _calendarEventRepository
                .GetEventByIdAsync(eventId)
                .Returns(Task.FromResult<CalendarEvent?>(calendarEvent));
            _calendarEventRepository
                .RetrieveInterestPairsAsync(eventId, parsedUserId)
                .Returns(Task.FromResult<EventInterest?>(existingInterest));
            _calendarEventRepository
                .GetInterestedUsersAsync(eventId)
                .Returns(Task.FromResult(expectedCountAfterRemove));
            _calendarEventRepository.SaveChangesAsync().Returns(Task.FromResult(1));

            // Act
            var result = await _service.ToggleInterestAsync(eventId, userIdString);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value.IsInterested, Is.False);
            Assert.That(result.Value.InterestedCount, Is.EqualTo(expectedCountAfterRemove));
            await _calendarEventRepository.Received(1).RemoveEventInterest(existingInterest);
            await _calendarEventRepository.Received(1).SaveChangesAsync();
        }

        [Test]
        public async Task ToggleInterestAsync_InvalidUserIdString_ReturnsNull()
        {
            // Arrange
            var eventId = 1;
            var invalidUserIdString = "abc";

            // Act
            var result = await _service.ToggleInterestAsync(eventId, invalidUserIdString);

            // Assert
            Assert.That(result, Is.Null);
            _logger
                .Received(1)
                .Log(
                    LogLevel.Warning,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => LogMessageContains(o, $"ToggleInterestAsync: Ugyldigt userIdString format eller værdi: {invalidUserIdString}"
                    )),
                    null,
                    Arg.Any<Func<object, Exception?, string>>()
                );
        }

        [Test]
        public async Task ToggleInterestAsync_UserNotFound_ReturnsNull()
        {
            // Arrange
            var eventId = 1;
            var userIdString = "123";

            _userManager.FindByIdAsync(userIdString).Returns(Task.FromResult<User?>(null));
            _calendarEventRepository
                .GetEventByIdAsync(eventId)
                .Returns(Task.FromResult<CalendarEvent?>(new CalendarEvent { Id = eventId }));

            // Act
            var result = await _service.ToggleInterestAsync(eventId, userIdString);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ToggleInterestAsync_EventNotFound_ReturnsNull()
        {
            // Arrange
            var eventId = 1;
            var userIdString = "123";
            var parsedUserId = 123;
            var user = new User { Id = parsedUserId, UserName = "TestUser" };

            _userManager.FindByIdAsync(userIdString).Returns(Task.FromResult<User?>(user));
            _calendarEventRepository
                .GetEventByIdAsync(eventId)
                .Returns(Task.FromResult<CalendarEvent?>(null));

            // Act
            var result = await _service.ToggleInterestAsync(eventId, userIdString);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ToggleInterestAsync_DbUpdateExceptionOnSave_ReturnsNullAndLogsError()
        {
            // Arrange
            var eventId = 1;
            var userIdString = "123";
            var parsedUserId = 123;
            var user = new User { Id = parsedUserId, UserName = "TestUser" };
            var calendarEvent = new CalendarEvent
            {
                Id = eventId,
                InterestedUsers = new List<EventInterest>(),
            };

            _userManager.FindByIdAsync(userIdString).Returns(Task.FromResult<User?>(user));
            _calendarEventRepository
                .GetEventByIdAsync(eventId)
                .Returns(Task.FromResult<CalendarEvent?>(calendarEvent));
            _calendarEventRepository
                .RetrieveInterestPairsAsync(eventId, parsedUserId)
                .Returns(Task.FromResult<EventInterest?>(null));
            _calendarEventRepository
                .SaveChangesAsync()
                .ThrowsAsync(new DbUpdateException("Simulated DB error"));

            // Act
            var result = await _service.ToggleInterestAsync(eventId, userIdString);

            // Assert
            Assert.That(result, Is.Null);
            _logger
                .Received(1)
                .Log(
                    LogLevel.Error,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o => LogMessageContains(o, "Failed to save changes after toggling interest.")
                    ),
                    Arg.Any<DbUpdateException>(), 
                    Arg.Any<Func<object, Exception?, string>>()
                );
        }

        #endregion

        #region GetAmountInterestedAsync Tests

        [Test]
        public async Task GetAmountInterestedAsync_ValidEventId_ReturnsCorrectCount()
        {
            // Arrange
            var eventId = 1;
            var expectedCount = 10;

            _calendarEventRepository
                .GetInterestedUsersAsync(eventId)
                .Returns(Task.FromResult(expectedCount));

            // Act
            var actualCount = await _service.GetAmountInterestedAsync(eventId);

            // Assert
            Assert.That(actualCount, Is.EqualTo(expectedCount));
            await _calendarEventRepository.Received(1).GetInterestedUsersAsync(eventId);
        }

        [Test]
        public async Task GetAmountInterestedAsync_RepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var eventId = 1;
            var exception = new InvalidOperationException("Database error");
            _calendarEventRepository.GetInterestedUsersAsync(eventId).ThrowsAsync(exception);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetAmountInterestedAsync(eventId)
            );
            await _calendarEventRepository.Received(1).GetInterestedUsersAsync(eventId);
        }

        #endregion
    }
}
