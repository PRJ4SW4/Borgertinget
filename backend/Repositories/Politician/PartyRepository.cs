using backend.Data;
using backend.Models.Politicians;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Politicians;

public class PartyRepository : IPartyRepository
{
    private readonly DataContext _context;
    private readonly ILogger<PartyRepository> _logger;

    public PartyRepository(DataContext context, ILogger<PartyRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Party>> GetAll()
    {
        var parties = await _context.Party.OrderBy(p => p.partyName).ToListAsync();
        return parties;
    }

    public async Task<Party?> GetById(int Id)
    {
        var party = await _context.Party.FindAsync(Id);

        if (party == null)
        {
            _logger.LogInformation("Unable to find Party");
            return null;
        }

        return party;
    }

    public async Task<Party?> GetByName(string PartyName)
    {
        var party = await _context.Party.FirstOrDefaultAsync(p =>
            p.partyName != null && p.partyName.ToLower() == PartyName.ToLower()
        );

        if (party == null)
        {
            _logger.LogInformation("No party found with name.");
            return null;
        }
        return party;
    }

    public async Task UpdatePartyDetail(Party party)
    {
        _context.Entry(party).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task AddParty(Party party)
    {
        _context.Party.Add(party);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveParty(Party party)
    {
        _context.Party.Remove(party);
        await _context.SaveChangesAsync();
    }

    public async Task AddMember(Party party, int MemberId)
    {
        party.memberIds ??= new List<int>();
        if (!party.memberIds.Contains(MemberId))
        {
            party.memberIds.Add(MemberId);
        }
        await _context.SaveChangesAsync();
    }

    public async Task<List<Party>> GetPartyByMemberId(int MemberId)
    {
        var partiesContainingAktor = await _context
            .Party.Where(p => p.memberIds != null && p.memberIds.Contains(MemberId))
            .ToListAsync();
        return partiesContainingAktor;
    }

    public async Task RemoveMember(Party party, int MemberId)
    {
        party.memberIds?.Remove(MemberId);
        _context.Entry(party).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
