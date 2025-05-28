using backend.DTOs;

namespace backend.Services.Feed
{
    public interface IFeedService
    {
        Task<List<PoliticianInfoDto>> GetUserSubscriptionsAsync(int userId);
        Task<PaginatedFeedResult> GetUserFeedAsync(
            int userId,
            int page,
            int pageSize,
            int? politicianId
        );
    }
}
