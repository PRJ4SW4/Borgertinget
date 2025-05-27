using backend.Controllers;
using backend.DTO.FT;
using backend.Services.Politicians;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Controllers;

[TestFixture]
public class PartyControllerTests
{
    private PartyController _uut;
    private IPartyService _service;

    [SetUp]
    public void SetUp()
    {
        _service = Substitute.For<IPartyService>();
        var logger = Substitute.For<ILogger<PartyController>>();
        _uut = new PartyController(_service, logger);
    }

    [Test]
    public async Task UpdatePartyDetails_ShouldReturnOk_WhenUpdateIsSuccessful()
    {
        // Arrange
        var partyId = 1;
        var updateDto = new UpdatePartyDto
        {
            partyId = 1,
            partyProgram = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
            politics = "Proin ac libero nec arcu vehicula tincidunt.",
            history = "Founded in 2025, the party has a rich history of innovation.",
        };
        _service.UpdateDetails(partyId, updateDto).Returns(true);

        // Act
        var result = await _uut.UpdatePartyDetails(partyId, updateDto);

        // Assert
        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo(true));
    }

    [Test]
    public async Task UpdatePartyDetails_ShouldReturnBadRequest_WhenDtoIsNull()
    {
        // Arrange
        var partyId = 1;

        // Act
        var result = await _uut.UpdatePartyDetails(partyId, null);

        // Assert
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        var badRequest = result.Result as BadRequestObjectResult;
        Assert.That(badRequest?.Value, Is.EqualTo("Update data cannot be null."));
    }

    [Test]
    public async Task UpdatePartyDetails_ShouldReturnBadRequest_WhenPartyIdIsInvalid()
    {
        // Arrange
        var partyId = -1;
        var updateDto = new UpdatePartyDto
        {
            partyId = 1,
            partyProgram = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
            politics = "Proin ac libero nec arcu vehicula tincidunt.",
            history = "Founded in 2025, the party has a rich history of innovation.",
        };

        // Act
        var result = await _uut.UpdatePartyDetails(partyId, updateDto);

        // Assert
        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        var badRequest = result.Result as BadRequestObjectResult;
        Assert.That(badRequest?.Value, Is.EqualTo("Invalid Party ID."));
    }

    [Test]
    public async Task UpdatePartyDetails_ShouldReturnInternalServerError_WhenServiceThrowsException()
    {
        // Arrange
        var partyId = 1;
        var updateDto = new UpdatePartyDto
        {
            partyId = 1,
            partyProgram = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
            politics = "Proin ac libero nec arcu vehicula tincidunt.",
            history = "Founded in 2025, the party has a rich history of innovation.",
        };
        _service
            .UpdateDetails(Arg.Any<int>(), Arg.Any<UpdatePartyDto>())
            .Returns(Task.FromException<bool>(new Exception("Unexpected error")));

        // Act
        var result = await _uut.UpdatePartyDetails(partyId, updateDto);

        // Assert
        Assert.That(result.Result, Is.TypeOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult?.Value, Is.EqualTo("An unexpected error occurred."));
    }
}
