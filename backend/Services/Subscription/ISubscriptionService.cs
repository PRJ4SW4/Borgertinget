using System.Threading.Tasks;
using backend.DTOs;

namespace backend.Services.Subscription
{
    public interface ISubscriptionService
    {
        Task<(bool success, string message)> SubscribeAsync(int userId, int politicianTwitterId);
        Task<(bool success, string message)> UnsubscribeAsync(int userId, int politicianTwitterId);
        Task<PoliticianInfoDto?> LookupPoliticianAsync(int aktorId);
    }
}
