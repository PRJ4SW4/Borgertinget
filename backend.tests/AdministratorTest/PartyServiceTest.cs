using backend.DTO.FT;
using backend.Models.Politicians;
using backend.Repositories.Politicians;
using backend.Services.Politicians;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Tests.Services;

[TestFixture]
public class PartyServiceTest
{
    private PartyService _service;
    private IPartyRepository _repository;
    private ILogger<PartyService> _logger;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IPartyRepository>();
        _logger = Substitute.For<ILogger<PartyService>>();
        _service = new PartyService(_repository, _logger);
    }

    [Test]
    public async Task UpdateDetails_ShouldReturnTrue_WhenUpdateIsSuccessful()
    {
        // Arrange
        var partyId = 1;
        var updateDto = new UpdatePartyDto
        {
            partyId = 1,
            partyProgram = "Updated program",
            politics = "Updated politics",
            history = "Updated history",
        };
        var party = new Party { partyId = partyId };

        _repository.GetById(partyId).Returns(party);
        _repository.SaveChangesAsync().Returns(1);

        // Act
        var result = await _service.UpdateDetails(partyId, updateDto);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(party.partyProgram, Is.EqualTo(updateDto.partyProgram));
        Assert.That(party.politics, Is.EqualTo(updateDto.politics));
        Assert.That(party.history, Is.EqualTo(updateDto.history));
    }

    [Test]
    public async Task UpdateDetails_ShouldReturnFalse_WhenPartyNotFound()
    {
        // Arrange
        var partyId = 1;
        var updateDto = new UpdatePartyDto
        {
            partyId = 1,
            partyProgram = "Updated program",
            politics = "Updated politics",
            history = "Updated history",
        };

        _repository.GetById(partyId).Returns((Party?)null);

        // Act
        var result = await _service.UpdateDetails(partyId, updateDto);

        // Assert
        Assert.That(result, Is.False);
    }
}
