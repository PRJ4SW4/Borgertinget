using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Controllers;
using backend.DTO.FT;
using backend.Services.Politicians;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace backend.Controllers.Tests
{
    [TestFixture]
    public class AktorControllerTests
    {
        private IFetchService _mockFetchService;
        private IAktorService _mockAktorService;
        private ILogger<AktorController> _mockLogger;
        private AktorController _uut;

        [SetUp]
        public void SetUp()
        {
            _mockFetchService = Substitute.For<IFetchService>();
            _mockAktorService = Substitute.For<IAktorService>();
            _mockLogger = Substitute.For<ILogger<AktorController>>();
            _uut = new AktorController(_mockFetchService, _mockAktorService, _mockLogger);
        }

        // --- Tests for GetAllAktors ---
        [Test]
        public async Task GetAllAktors_WhenServiceReturnsAktors_ReturnsOkWithAktors()
        {
            // Arrange
            var expectedAktors = new List<AktorDetailDto>
            {
                new AktorDetailDto { Id = 1, navn = "Aktor One" },
                new AktorDetailDto { Id = 2, navn = "Aktor Two" },
            };
            _mockAktorService.getAllAktors().Returns(Task.FromResult(expectedAktors));

            // Act
            var actionResult = await _uut.GetAllAktors();

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null, "Result should be OkObjectResult.");
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.EqualTo(expectedAktors));
        }

        [Test]
        public async Task GetAllAktors_WhenServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyList = new List<AktorDetailDto>();
            _mockAktorService.getAllAktors().Returns(Task.FromResult(emptyList));

            // Act
            var actionResult = await _uut.GetAllAktors();

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null, "Result should be OkObjectResult.");
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            var returnedList = okResult.Value as IEnumerable<AktorDetailDto>;
            Assert.That(returnedList, Is.Not.Null);
            Assert.That(returnedList, Is.Empty);
        }

        [Test]
        public async Task GetAllAktors_WhenServiceThrowsException_ReturnsStatusCode500()
        {
            // Arrange
            _mockAktorService.getAllAktors().ThrowsAsync(new Exception("Service error"));

            // Act
            var actionResult = await _uut.GetAllAktors();

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null, "Result should be ObjectResult for error.");
            Assert.That(
                statusCodeResult.StatusCode,
                Is.EqualTo(StatusCodes.Status500InternalServerError)
            );
            Assert.That(
                statusCodeResult.Value,
                Is.EqualTo("An error occured while fetching aktors")
            );
        }

        // --- Tests for GetAktorById ---
        [Test]
        public async Task GetAktorById_WhenAktorExists_ReturnsOkWithAktor()
        {
            // Arrange
            int aktorId = 1;
            var expectedAktor = new AktorDetailDto { Id = aktorId, navn = "Test Aktor" };
            _mockAktorService
                .getById(aktorId)
                .Returns(Task.FromResult<AktorDetailDto?>(expectedAktor));

            // Act
            var actionResult = await _uut.GetAktorById(aktorId);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null, "Result should be OkObjectResult.");
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.EqualTo(expectedAktor));
        }

        [Test]
        public async Task GetAktorById_WhenAktorDoesNotExist_ReturnsOkWithNull()
        {
            // Arrange
            int aktorId = 99;
            _mockAktorService.getById(aktorId).Returns(Task.FromResult<AktorDetailDto?>(null));

            // Act
            var actionResult = await _uut.GetAktorById(aktorId);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(
                okResult,
                Is.Not.Null,
                "Result should be OkObjectResult even for null service response."
            );
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.Null);
        }

        [Test]
        public async Task GetAktorById_WhenServiceThrowsException_ReturnsStatusCode500()
        {
            // Arrange
            int aktorId = 1;
            _mockAktorService.getById(aktorId).ThrowsAsync(new Exception("Service error"));

            // Act
            var actionResult = await _uut.GetAktorById(aktorId);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null, "Result should be ObjectResult for error.");
            Assert.That(
                statusCodeResult.StatusCode,
                Is.EqualTo(StatusCodes.Status500InternalServerError)
            );
            Assert.That(
                statusCodeResult.Value,
                Is.EqualTo("An error occured while processing your request")
            );
        }

        // --- Tests for GetParty ---
        [Test]
        public async Task GetParty_WhenAktorsExistForParty_ReturnsOkWithAktors()
        {
            // Arrange
            string partyName = "Test Party";
            var expectedAktors = new List<AktorDetailDto>
            {
                new AktorDetailDto { Id = 1, Party = partyName },
            };
            _mockAktorService.getByParty(partyName).Returns(Task.FromResult(expectedAktors));

            // Act
            var actionResult = await _uut.GetParty(partyName);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null, "Result should be OkObjectResult.");
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.EqualTo(expectedAktors));
        }

        [Test]
        public async Task GetParty_WhenNoAktorsForParty_ReturnsOkWithEmptyList()
        {
            // Arrange
            string partyName = "Empty Party";
            _mockAktorService
                .getByParty(partyName)
                .Returns(Task.FromResult(new List<AktorDetailDto>()));

            // Act
            var actionResult = await _uut.GetParty(partyName);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null, "Result should be OkObjectResult.");
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            var returnedList = okResult.Value as IEnumerable<AktorDetailDto>;
            Assert.That(returnedList, Is.Not.Null);
            Assert.That(returnedList, Is.Empty);
        }

        [Test]
        public async Task GetParty_WhenServiceThrowsException_ReturnsStatusCode500()
        {
            // Arrange
            string partyName = "Error Party";
            _mockAktorService.getByParty(partyName).ThrowsAsync(new Exception("Service error"));

            // Act
            var actionResult = await _uut.GetParty(partyName);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null, "Result should be ObjectResult for error.");
            Assert.That(
                statusCodeResult.StatusCode,
                Is.EqualTo(StatusCodes.Status500InternalServerError)
            );
            Assert.That(
                statusCodeResult.Value,
                Is.EqualTo("An error occurred while processing your request.")
            );
        }

        // --- Tests for UpdateAktorsFromExternal ---
        [Test]
        public async Task UpdateAktorsFromExternal_SuccessfulFetch_ReturnsOkWithMessage()
        {
            // Arrange
            int added = 5,
                updated = 10,
                deleted = 2;
            _mockFetchService
                .FetchAndUpdateAktorsAsync()
                .Returns(Task.FromResult((added, updated, deleted)));

            // Act
            var actionResult = await _uut.UpdateAktorsFromExternal();

            // Assert
            Assert.That(actionResult, Is.InstanceOf<OkObjectResult>());
            var okResult = actionResult as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(
                okResult.Value?.ToString(),
                Does.Contain(
                    $"Successfully added {added}, updated {updated}, and deleted {deleted} aktors."
                )
            );

            _mockLogger
                .Received(1)
                .Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o =>
                        o != null && o.ToString()!.Contains("Aktor update process completed.")
                    ),
                    null,
                    Arg.Any<Func<object, Exception?, string>>()
                );
        }

        [Test]
        public async Task UpdateAktorsFromExternal_FetchServiceThrowsInvalidOperationException_ReturnsStatusCode500WithSpecificMessage()
        {
            // Arrange
            var exceptionMessage = "API URL configuration error";
            var ex = new InvalidOperationException(exceptionMessage);
            _mockFetchService.FetchAndUpdateAktorsAsync().ThrowsAsync(ex);

            // Act
            var actionResult = await _uut.UpdateAktorsFromExternal();

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ObjectResult>());
            var objectResult = actionResult as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(
                objectResult.StatusCode,
                Is.EqualTo(StatusCodes.Status500InternalServerError)
            );
            dynamic? value = objectResult.Value;
            Assert.That(value, Is.Not.Null);
            Assert.That(
                (string)value.GetType().GetProperty("message").GetValue(value, null),
                Is.EqualTo("Server configuration error for Aktor update.")
            );
            Assert.That(
                (string)value.GetType().GetProperty("error").GetValue(value, null),
                Is.EqualTo(exceptionMessage)
            );

            _mockLogger
                .Received(1)
                .Log(
                    LogLevel.Error,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o =>
                        o != null
                        && o.ToString()!.Contains("Configuration error during Aktor update.")
                    ),
                    Arg.Is(ex),
                    Arg.Any<Func<object, Exception?, string>>()
                );
        }

        [Test]
        public async Task UpdateAktorsFromExternal_FetchServiceThrowsGenericException_ReturnsStatusCode500WithGenericMessage()
        {
            // Arrange
            var exceptionMessage = "A general processing error occurred.";
            var ex = new Exception(exceptionMessage);
            _mockFetchService.FetchAndUpdateAktorsAsync().ThrowsAsync(ex);

            // Act
            var actionResult = await _uut.UpdateAktorsFromExternal();

            // Assert
            Assert.That(actionResult, Is.InstanceOf<ObjectResult>());
            var objectResult = actionResult as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(
                objectResult.StatusCode,
                Is.EqualTo(StatusCodes.Status500InternalServerError)
            );
            dynamic? value = objectResult.Value;
            Assert.That(value, Is.Not.Null);
            Assert.That(
                (string)value.GetType().GetProperty("message").GetValue(value, null),
                Is.EqualTo("An error occurred during the Aktor update process.")
            );
            Assert.That(
                (string)value.GetType().GetProperty("error").GetValue(value, null),
                Is.EqualTo(exceptionMessage)
            );

            _mockLogger
                .Received(1)
                .Log(
                    LogLevel.Error,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o =>
                        o != null
                        && o.ToString()!
                            .Contains(
                                "An error occurred while updating Aktors from external source."
                            )
                    ),
                    Arg.Is(ex),
                    Arg.Any<Func<object, Exception?, string>>()
                );
        }
    }
}
