using backend.DTO.FT;
using backend.Models.Politicians;

namespace backend.Services.Politicians;

public interface IPartyService
{
    public Task<bool> UpdateDetails(int Id, PartyDto party);
    public Task<Party?> GetById(int Id);
    public Task<List<Party>?> GetAll();
    public Task Add(Party party);
    public Task Remove(Party party);
    public Task AddMember(Party party, int MemberId);
    public Task removeMember(Party party, int MemberId);
    public Task<Party?> GetByName(string partyName);
}
