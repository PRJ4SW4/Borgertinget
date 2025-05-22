using backend.DTO.FT;
using backend.Models.Politicians;
using backend.Repositories.Politicians;

namespace backend.Services.Politicians;

public class PartyService : IPartyService
{
    private readonly IPartyRepository _repo;
    private readonly ILogger<PartyService> _logger;

    public PartyService(IPartyRepository repo, ILogger<PartyService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<bool> UpdateDetails(int Id, UpdatePartyDto dto)
    {
        var party = await _repo.GetById(Id);
        if (party == null)
        {
            _logger.LogInformation("No Party found");
            return false;
        }
        party.partyProgram = dto.partyProgram;
        party.politics = dto.politics;
        party.history = dto.history;

        await _repo.UpdatePartyDetail(party); // Mark as edited
        int changes = await _repo.SaveChangesAsync(); // Save changes

        return changes > 0;
    }

    public async Task<List<PartyDetailsDto>?> GetAll()
    {
        var parties = await _repo.GetAll();
        if (parties == null)
        {
            return null;
        }
        return parties.Select(p => MapToPartyDetailsDto(p)).ToList();
    }

    public async Task Add(Party party)
    {
        await _repo.AddParty(party);
    }

    public async Task Remove(Party party)
    {
        await _repo.RemoveParty(party);
    }

    public async Task AddMember(Party party, int MemberId)
    {
        await _repo.AddMember(party, MemberId);
    }

    public async Task removeMember(Party party, int MemberId)
    {
        await _repo.RemoveMember(party, MemberId);
    }

    public async Task<PartyDetailsDto?> GetByName(string partyName)
    {
        var party = await _repo.GetByName(partyName);

        if (party == null)
        {
            _logger.LogInformation("Unable to find party");
            return null;
        }
        return MapToPartyDetailsDto(party);
    }

    public PartyDetailsDto MapToPartyDetailsDto(Party party)
    {
        return new PartyDetailsDto
        {
            partyId = party.partyId,
            partyName = party.partyName,
            partyShortName = party.partyShortName,
            partyProgram = party.partyProgram,
            politics = party.politics,
            history = party.history,
            stats = party.stats,
            chairmanId = party.chairmanId,
            viceChairmanId = party.viceChairmanId,
            secretaryId = party.secretaryId,
            spokesmanId = party.spokesmanId,
            memberIds = party.memberIds,
        };
    }
}
