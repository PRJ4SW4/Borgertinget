using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Controllers; // Assuming PartyController is here
using backend.DTO.FT; // Assuming PartyDetailsDto and UpdatePartyDto are here
using backend.Services.Politicians; // Assuming IPartyService is here
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // For DbUpdateException and DbUpdateConcurrencyException
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions; // For ThrowsAsync
using NUnit.Framework;

// Ensure DTOs are correctly referenced from your main project.
// If they are not in backend.DTO.FT, adjust the using statement.

namespace backend.Controllers.Tests
{
    [TestFixture]
    public class PartyControllerTests
    {
        private IPartyService _mockPartyService;
        private ILogger<PartyController> _mockLogger;
        private PartyController _uut;

        [SetUp]
        public void SetUp()
        {
            _mockPartyService = Substitute.For<IPartyService>();
            _mockLogger = Substitute.For<ILogger<PartyController>>();
            _uut = new PartyController(_mockPartyService, _mockLogger);
        }

        // --- Tests for getParties ---
        [Test]
        public async Task GetParties_WhenServiceReturnsParties_ReturnsOkWithParties()
        {
            // Arrange
            var expectedParties = new List<PartyDetailsDto>
            {
                new PartyDetailsDto { partyId = 1, partyName = "Party A" },
                new PartyDetailsDto { partyId = 2, partyName = "Party B" },
            };
            _mockPartyService
                .GetAll()
                .Returns(Task.FromResult<List<PartyDetailsDto>?>(expectedParties));

            // Act
            var actionResult = await _uut.getParties();

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null, "Result should be OkObjectResult.");
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.EqualTo(expectedParties));
        }

        [Test]
        public async Task GetParties_WhenServiceReturnsNull_ReturnsStatusCode404()
        {
            // Arrange
            _mockPartyService.GetAll().Returns(Task.FromResult<List<PartyDetailsDto>?>(null));

            // Act
            var actionResult = await _uut.getParties();

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.That(
                statusCodeResult,
                Is.Not.Null,
                "Expected ObjectResult for null service response."
            );
            Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
            Assert.That(statusCodeResult.Value, Is.EqualTo("No parties found"));
        }

        [Test]
        public async Task GetParties_WhenServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyList = new List<PartyDetailsDto>();
            _mockPartyService.GetAll().Returns(Task.FromResult<List<PartyDetailsDto>?>(emptyList));

            // Act
            var actionResult = await _uut.getParties();

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null, "Result should be OkObjectResult.");
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            var returnedList = okResult.Value as IEnumerable<PartyDetailsDto>;
            Assert.That(returnedList, Is.Not.Null);
            Assert.That(returnedList, Is.Empty);
        }

        // --- Tests for GetPartyByName ---
        [Test]
        public async Task GetPartyByName_ValidName_PartyExists_ReturnsOkWithParty()
        {
            // Arrange
            string partyName = "TestParty";
            var expectedParty = new PartyDetailsDto { partyId = 1, partyName = partyName };
            _mockPartyService
                .GetByName(partyName)
                .Returns(Task.FromResult<PartyDetailsDto?>(expectedParty));

            // Act
            var actionResult = await _uut.GetPartyByName(partyName);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null, "Result should be OkObjectResult.");
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.EqualTo(expectedParty));
        }

        [Test]
        public async Task GetPartyByName_ValidName_PartyDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            string partyName = "NonExistentParty";
            _mockPartyService.GetByName(partyName).Returns(Task.FromResult<PartyDetailsDto?>(null));

            // Act
            var actionResult = await _uut.GetPartyByName(partyName);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            Assert.That(actionResult.Result, Is.InstanceOf<NotFoundObjectResult>());
            var notFoundResult = actionResult.Result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null, "Result should be NotFoundObjectResult.");
            Assert.That(notFoundResult.Value, Is.EqualTo("Party not found."));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public async Task GetPartyByName_InvalidName_ReturnsBadRequest(string? invalidPartyName)
        {
            // Act
            var actionResult = await _uut.GetPartyByName(invalidPartyName!);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            Assert.That(actionResult.Result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = actionResult.Result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null, "Result should be BadRequestObjectResult.");
            Assert.That(badRequestResult.Value, Is.EqualTo("Party name cannot be empty."));
        }

        [Test]
        public async Task GetPartyByName_ServiceThrowsException_ReturnsStatusCode500()
        {
            // Arrange
            string partyName = "ErrorParty";
            _mockPartyService
                .GetByName(partyName)
                .ThrowsAsync(new Exception("Service layer error"));

            // Act
            var actionResult = await _uut.GetPartyByName(partyName);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null, "Result should be ObjectResult for error.");
            Assert.That(
                statusCodeResult.StatusCode,
                Is.EqualTo(StatusCodes.Status500InternalServerError)
            );
            Assert.That(statusCodeResult.Value, Is.EqualTo("Internal server error."));
        }

        // --- Tests for UpdatePartyDetails ---
        [Test]
        public async Task UpdatePartyDetails_ValidInput_ServiceReturnsTrue_ReturnsOkWithTrue()
        {
            // Arrange
            int partyId = 1;
            var updateDto = new UpdatePartyDto { partyProgram = "Updated" };
            _mockPartyService.UpdateDetails(partyId, updateDto).Returns(Task.FromResult(true));

            // Act
            var actionResult = await _uut.UpdatePartyDetails(partyId, updateDto);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null, "Result should be OkObjectResult.");
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.EqualTo(true));
        }

        [Test]
        public async Task UpdatePartyDetails_ValidInput_ServiceReturnsFalse_ReturnsOkWithFalse()
        {
            // Arrange
            int partyId = 1;
            var updateDto = new UpdatePartyDto { partyProgram = "Updated" };
            _mockPartyService.UpdateDetails(partyId, updateDto).Returns(Task.FromResult(false));

            // Act
            var actionResult = await _uut.UpdatePartyDetails(partyId, updateDto);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var okResult = actionResult.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null, "Result should be OkObjectResult.");
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(okResult.Value, Is.EqualTo(false));
        }

        [Test]
        public async Task UpdatePartyDetails_NullDto_ReturnsBadRequest()
        {
            // Act
            var actionResult = await _uut.UpdatePartyDetails(1, null!);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            Assert.That(actionResult.Result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = actionResult.Result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null, "Result should be BadRequestObjectResult.");
            Assert.That(badRequestResult.Value, Is.EqualTo("Update data cannot be null."));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public async Task UpdatePartyDetails_InvalidPartyId_ReturnsBadRequest(int invalidPartyId)
        {
            // Arrange
            var updateDto = new UpdatePartyDto();

            // Act
            var actionResult = await _uut.UpdatePartyDetails(invalidPartyId, updateDto);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            Assert.That(actionResult.Result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = actionResult.Result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null, "Result should be BadRequestObjectResult.");
            Assert.That(badRequestResult.Value, Is.EqualTo("Invalid Party ID."));
        }

        [Test]
        public async Task UpdatePartyDetails_ServiceThrowsDbConcurrencyException_ReturnsStatusCode500()
        {
            // Arrange
            int partyId = 1;
            var updateDto = new UpdatePartyDto();
            _mockPartyService
                .UpdateDetails(partyId, updateDto)
                .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency error"));

            // Act
            var actionResult = await _uut.UpdatePartyDetails(partyId, updateDto);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null, "Result should be ObjectResult for error.");
            Assert.That(
                statusCodeResult.StatusCode,
                Is.EqualTo(StatusCodes.Status500InternalServerError)
            );
            Assert.That(statusCodeResult.Value, Does.Contain("A concurrency error occurred"));
        }

        [Test]
        public async Task UpdatePartyDetails_ServiceThrowsDbUpdateException_ReturnsStatusCode500()
        {
            // Arrange
            int partyId = 1;
            var updateDto = new UpdatePartyDto();
            _mockPartyService
                .UpdateDetails(partyId, updateDto)
                .ThrowsAsync(new DbUpdateException("DB update error"));

            // Act
            var actionResult = await _uut.UpdatePartyDetails(partyId, updateDto);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null, "Result should be ObjectResult for error.");
            Assert.That(
                statusCodeResult.StatusCode,
                Is.EqualTo(StatusCodes.Status500InternalServerError)
            );
            Assert.That(statusCodeResult.Value, Does.Contain("A database error occurred"));
        }

        [Test]
        public async Task UpdatePartyDetails_ServiceThrowsGenericException_ReturnsStatusCode500()
        {
            // Arrange
            int partyId = 1;
            var updateDto = new UpdatePartyDto();
            _mockPartyService
                .UpdateDetails(partyId, updateDto)
                .ThrowsAsync(new Exception("Generic error"));

            // Act
            var actionResult = await _uut.UpdatePartyDetails(partyId, updateDto);

            // Assert
            Assert.That(actionResult, Is.Not.Null);
            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null, "Result should be ObjectResult for error.");
            Assert.That(
                statusCodeResult.StatusCode,
                Is.EqualTo(StatusCodes.Status500InternalServerError)
            );
            Assert.That(statusCodeResult.Value, Does.Contain("An unexpected error occurred"));
        }
    }
}
