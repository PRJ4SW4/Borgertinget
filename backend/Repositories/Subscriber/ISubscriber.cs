using System.Threading.Tasks;
using backend.Models;

namespace backend.Repositories.Subscription
{
    public interface ISubscriptionRepository
    {
        Task<bool> SubscribeAsync(int userId, int politicianTwitterId); // Handles all subscription logic
        Task<bool> UnsubscribeAsync(int userId, int politicianTwitterId); // Handles all unsubscribe logic
        Task<PoliticianTwitterId?> LookupPoliticianAsync(int aktorId); // Handles lookup logic
    }
}
