using backend.DTO.FT;
using backend.Repositories.Politicians;

namespace backend.Services.Politicians;

public class AktorService : IAktorService
{
    private readonly IAktorRepo _repo;
    private readonly ILogger<AktorService> _logger;

    public AktorService(IAktorRepo repo, ILogger<AktorService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<AktorDetailDto?> getById(int Id)
    {
        var aktor = await _repo.GetAktorByIdAsync(Id);

        if (aktor == null)
        {
            _logger.LogError("Error fetching Aktor");
            return null;
        }
        var aktorDto = AktorDetailDto.FromAktor(aktor);

        return aktorDto;
    }

    public async Task<List<AktorDetailDto>> getAllAktors()
    {
        var aktors = await _repo.AllAktorsToList();
        List<AktorDetailDto> aktorDtos = new List<AktorDetailDto>();
        foreach (var aktor in aktors)
        {
            aktorDtos.Add(AktorDetailDto.FromAktor(aktor));
        }

        return aktorDtos;
    }

    public async Task<List<AktorDetailDto>> getByParty(string party)
    {
        var aktors = await _repo.GetAktorsByParty(party);
        List<AktorDetailDto> aktorDtos = new List<AktorDetailDto>();
        foreach (var aktor in aktors)
        {
            aktorDtos.Add(AktorDetailDto.FromAktor(aktor)); //Map til aktorDetailDto
        }
        return aktorDtos;
    }
}
