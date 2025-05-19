using System.Threading.Tasks;

namespace backend.Services.Subscription
{
    public interface ISubscriptionService
    {
        Task<(bool success, string message)> SubscribeAsync(int userId, int politicianTwitterId);
        Task<(bool success, string message)> UnsubscribeAsync(int userId, int politicianTwitterId);
        Task<(bool success, object? result, string? message)> LookupPoliticianAsync(int aktorId);
    }
}