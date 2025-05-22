using backend.DTO.FT;
using backend.Models.Politicians;

namespace backend.Services.Politicians;

public interface IPartyService
{
    public Task<bool> UpdateDetails(int Id, UpdatePartyDto party);
    public Task<PartyDetailsDto?> GetByName(string partyName);
    public Task<List<PartyDetailsDto>?> GetAll();
    public Task Add(Party party);
    public Task Remove(Party party);
    public Task AddMember(Party party, int MemberId);
    public Task removeMember(Party party, int MemberId);
    PartyDetailsDto MapToPartyDetailsDto(Party party);
}
