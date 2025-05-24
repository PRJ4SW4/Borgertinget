using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTO.FT;
using backend.Models.Politicians;
using backend.Repositories.Politicians;
using backend.Services.Politicians;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace backend.Services.Tests
{
    [TestFixture]
    public class PartyServiceTests
    {
        private IPartyRepository _mockPartyRepo;
        private ILogger<PartyService> _mockLogger;
        private PartyService _uut;

        [SetUp]
        public void SetUp()
        {
            _mockPartyRepo = Substitute.For<IPartyRepository>();
            _mockLogger = Substitute.For<ILogger<PartyService>>();
            _uut = new PartyService(_mockPartyRepo, _mockLogger);
        }

        // --- Test for UpdateDetails ---
        [Test]
        public async Task UpdateDetails_PartyExists_UpdatesAndSaves_ReturnsTrue()
        {
            // Arrange
            int partyId = 1;
            var updateDto = new UpdatePartyDto
            {
                partyProgram = "New Program",
                politics = "New Politics",
                history = "New History",
            };
            var existingParty = new Party { partyId = partyId, partyProgram = "Old Program" };

            _mockPartyRepo.GetById(partyId).Returns(Task.FromResult<Party?>(existingParty));
            _mockPartyRepo.SaveChangesAsync().Returns(Task.FromResult(1));

            // Act
            var result = await _uut.UpdateDetails(partyId, updateDto);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(existingParty.partyProgram, Is.EqualTo(updateDto.partyProgram));
            Assert.That(existingParty.politics, Is.EqualTo(updateDto.politics));
            Assert.That(existingParty.history, Is.EqualTo(updateDto.history));
            await _mockPartyRepo.Received(1).UpdatePartyDetail(existingParty);
            await _mockPartyRepo.Received(1).SaveChangesAsync();
        }

        [Test]
        public async Task UpdateDetails_PartyDoesNotExist_LogsAndReturnsFalse()
        {
            // Arrange
            int partyId = 99;
            var updateDto = new UpdatePartyDto();
            _mockPartyRepo.GetById(partyId).Returns(Task.FromResult<Party?>(null));

            // Act
            var result = await _uut.UpdateDetails(partyId, updateDto);

            // Assert
            Assert.That(result, Is.False);
            _mockLogger.Received(1).LogInformation("No Party found");
            await _mockPartyRepo.DidNotReceiveWithAnyArgs().UpdatePartyDetail(Arg.Any<Party>());
            await _mockPartyRepo.DidNotReceiveWithAnyArgs().SaveChangesAsync();
        }

        [Test]
        public async Task UpdateDetails_NoChangesMadeBySaveChanges_ReturnsFalse()
        {
            // Arrange
            int partyId = 1;
            var updateDto = new UpdatePartyDto { partyProgram = "Updated Program" };
            var existingParty = new Party { partyId = partyId, partyProgram = "Old Program" };

            _mockPartyRepo.GetById(partyId).Returns(Task.FromResult<Party?>(existingParty));
            _mockPartyRepo.SaveChangesAsync().Returns(Task.FromResult(0));

            // Act
            var result = await _uut.UpdateDetails(partyId, updateDto);

            // Assert
            Assert.That(result, Is.False);
            await _mockPartyRepo.Received(1).UpdatePartyDetail(existingParty);
            await _mockPartyRepo.Received(1).SaveChangesAsync();
        }

        // --- Test for GetAll ---
        [Test]
        public async Task GetAll_WhenPartiesExist_ReturnsListOfPartyDetailsDto()
        {
            // Arrange
            var partyList = new List<Party>
            {
                new Party { partyId = 1, partyName = "Party A" },
                new Party { partyId = 2, partyName = "Party B" },
            };
            _mockPartyRepo.GetAll().Returns(Task.FromResult(partyList));

            // Act
            var result = await _uut.GetAll();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(partyList.Count));
            Assert.That(result[0].partyName, Is.EqualTo(partyList[0].partyName));
        }

        [Test]
        public async Task GetAll_WhenNoPartiesExist_ReturnsNull()
        {
            // Arrange
            _mockPartyRepo.GetAll().Returns(Task.FromResult<List<Party>>(null!));

            // Act
            var result = await _uut.GetAll();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAll_WhenEmptyListReturnedByRepo_ReturnsEmptyDtoList()
        {
            // Arrange
            _mockPartyRepo.GetAll().Returns(Task.FromResult(new List<Party>()));

            // Act
            var result = await _uut.GetAll();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        // --- Test for Add ---
        [Test]
        public async Task Add_CallsRepositoryAddParty()
        {
            // Arrange
            var newParty = new Party { partyName = "New Party" };

            // Act
            await _uut.Add(newParty);

            // Assert
            await _mockPartyRepo.Received(1).AddParty(newParty);
        }

        // --- Test for Remove ---
        [Test]
        public async Task Remove_CallsRepositoryRemoveParty()
        {
            // Arrange
            var partyToRemove = new Party { partyId = 1, partyName = "Party To Remove" };

            // Act
            await _uut.Remove(partyToRemove);

            // Assert
            await _mockPartyRepo.Received(1).RemoveParty(partyToRemove);
        }

        // --- Test for AddMember ---
        [Test]
        public async Task AddMember_CallsRepositoryAddMember()
        {
            // Arrange
            var party = new Party { partyId = 1 };
            int memberId = 100;

            // Act
            await _uut.AddMember(party, memberId);

            // Assert
            await _mockPartyRepo.Received(1).AddMember(party, memberId);
        }

        // --- Test for removeMember ---
        [Test]
        public async Task RemoveMember_CallsRepositoryRemoveMember()
        {
            // Arrange
            var party = new Party { partyId = 1 };
            int memberId = 100;

            // Act
            await _uut.removeMember(party, memberId);

            // Assert
            await _mockPartyRepo.Received(1).RemoveMember(party, memberId);
        }

        // --- Test for GetByName ---
        [Test]
        public async Task GetByName_PartyExists_ReturnsPartyDetailsDto()
        {
            // Arrange
            string partyName = "Existing Party";
            var partyEntity = new Party
            {
                partyId = 1,
                partyName = partyName,
                partyShortName = "EP",
            };
            _mockPartyRepo.GetByName(partyName).Returns(Task.FromResult<Party?>(partyEntity));

            // Act
            var result = await _uut.GetByName(partyName);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.partyId, Is.EqualTo(partyEntity.partyId));
            Assert.That(result.partyName, Is.EqualTo(partyEntity.partyName));
            _mockLogger
                .DidNotReceive()
                .Log(
                    LogLevel.Information,
                    Arg.Any<EventId>(),
                    Arg.Is<object>(o =>
                        o != null && o.ToString()!.Contains("Unable to find party")
                    ), // Added null check for o
                    null, // Exception can be null
                    Arg.Any<Func<object, Exception?, string>>()
                );
        }

        [Test]
        public async Task GetByName_PartyDoesNotExist_ReturnsNullAndLogs()
        {
            // Arrange
            string partyName = "NonExistent Party";
            _mockPartyRepo.GetByName(partyName).Returns(Task.FromResult<Party?>(null));

            // Act
            var result = await _uut.GetByName(partyName);

            // Assert
            Assert.That(result, Is.Null);
            _mockLogger.Received(1).LogInformation("Unable to find party");
        }

        // --- Test for MapToPartyDetailsDto ---
        [Test]
        public void MapToPartyDetailsDto_CorrectlyMapsPartyToDto()
        {
            // Arrange
            var partyEntity = new Party
            {
                partyId = 10,
                partyName = "Mapping Test Party",
                partyShortName = "MTP",
                partyProgram = "Test Program",
                politics = "Test Politics",
                history = "Test History",
                stats = new List<string> { "Stat1", "Stat2" },
                chairmanId = 101,
                viceChairmanId = 102,
                secretaryId = 103,
                spokesmanId = 104,
                memberIds = new List<int> { 201, 202 },
            };

            // Act
            var dto = _uut.MapToPartyDetailsDto(partyEntity);

            // Assert
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto.partyId, Is.EqualTo(partyEntity.partyId));
            Assert.That(dto.partyName, Is.EqualTo(partyEntity.partyName));
            Assert.That(dto.partyShortName, Is.EqualTo(partyEntity.partyShortName));
            Assert.That(dto.partyProgram, Is.EqualTo(partyEntity.partyProgram));
            Assert.That(dto.politics, Is.EqualTo(partyEntity.politics));
            Assert.That(dto.history, Is.EqualTo(partyEntity.history));
            Assert.That(dto.stats, Is.EqualTo(partyEntity.stats));
            Assert.That(dto.chairmanId, Is.EqualTo(partyEntity.chairmanId));
            Assert.That(dto.viceChairmanId, Is.EqualTo(partyEntity.viceChairmanId));
            Assert.That(dto.secretaryId, Is.EqualTo(partyEntity.secretaryId));
            Assert.That(dto.spokesmanId, Is.EqualTo(partyEntity.spokesmanId));
            Assert.That(dto.memberIds, Is.EqualTo(partyEntity.memberIds));
        }
    }
}
