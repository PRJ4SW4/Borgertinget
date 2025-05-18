using backend.Data;
using backend.Models.Politicians;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Politicians;

public class AktorRepo : IAktorRepo
{
    private readonly DataContext _context;
    private readonly ILogger<AktorRepo> _logger;

    public AktorRepo(DataContext context, ILogger<AktorRepo> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Aktor?> GetAktorByIdAsync(int Id)
    {
        try
        {
            var aktor = await _context.Aktor.FindAsync(Id);

            return aktor;
        }
        catch (Exception)
        {
            _logger.LogError("Error fetching politician");
            return null;
        }
    }

    public async Task AddAktor(Aktor aktor)
    {
        _context.Aktor.Add(aktor);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAktor(Aktor aktor)
    {
        _context.Aktor.Remove(aktor);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAktor(Aktor aktor)
    {
        _context.Entry(aktor).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task<List<Aktor>> AllAktorsToList()
    {
        var aktors = await _context
            .Aktor.Where(a => a.typeid == 5)
            .OrderBy(a => a.navn)
            .ToListAsync();

        if (!aktors.Any())
        {
            _logger.LogInformation("No Aktors were found");
        }
        return aktors;
    }

    public async Task<List<Aktor>> GetAktorsByParty(string party)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(party))
        {
            _logger.LogInformation("Party name cannot be empty.");
        }

        // Normalize to lowercase
        var lowerPartyName = party.ToLower();
        // Query
        var filteredPoliticians = await _context
            .Aktor
            // Filter
            .Where(a =>
                (a.Party != null && a.Party.ToLower() == lowerPartyName)
                || (a.PartyShortname != null && a.PartyShortname.ToLower() == lowerPartyName)
            )
            .OrderBy(a => a.navn)
            .ToListAsync();

        return filteredPoliticians;
    }
}
