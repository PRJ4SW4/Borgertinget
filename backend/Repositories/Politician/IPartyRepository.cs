using backend.Models.Politicians;

namespace backend.Repositories.Politicians;

public interface IPartyRepository
{
    public Task<List<Party>> GetAll();
    public Task<Party?> GetByName(string partyName);
    public Task<Party?> GetById(int Id);
    public Task UpdatePartyDetail(Party party);
    public Task AddParty(Party party);
    public Task RemoveParty(Party party);
    public Task AddMember(Party party, int MemberId);
    public Task<List<Party>> GetPartyByMemberId(int MemberId);
    public Task RemoveMember(Party party, int MemberId);
}
