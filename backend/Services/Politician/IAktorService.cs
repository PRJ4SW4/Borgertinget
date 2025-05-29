using backend.DTO.FT;

namespace backend.Services.Politicians;

public interface IAktorService
{
    public Task<AktorDetailDto?> getById(int Id);
    public Task<List<AktorDetailDto>> getAllAktors();
    public Task<List<AktorDetailDto>> getByParty(string party);
}
