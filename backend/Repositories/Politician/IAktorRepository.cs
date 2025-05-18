using backend.Models.Politicians;

namespace backend.Repositories.Politicians
{
    public interface IAktorRepo
    {
        Task<Aktor?> GetAktorByIdAsync(int Id);
        Task AddAktor(Aktor aktor);
        Task DeleteAktor(Aktor aktor);
        Task UpdateAktor(Aktor aktor);
        Task<List<Aktor>> AllAktorsToList();
        Task<List<Aktor>> GetAktorsByParty(string party);
    }
}
