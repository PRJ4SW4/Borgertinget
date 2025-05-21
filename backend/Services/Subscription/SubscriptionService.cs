using System.Threading.Tasks;
using backend.Repositories.Subscription;
using Microsoft.Extensions.Logging;

namespace backend.Services.Subscription
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _repository;

        public SubscriptionService(ISubscriptionRepository repository)
        {
            _repository = repository;
        }

        public async Task<(bool success, string message)> SubscribeAsync(
            int userId,
            int politicianTwitterId
        )
        {
            var success = await _repository.SubscribeAsync(userId, politicianTwitterId);

            if (!success)
            {
                var politicianExists =
                    await _repository.LookupPoliticianAsync(politicianTwitterId) != null;
                if (!politicianExists)
                    return (false, $"Politiker med ID {politicianTwitterId} findes ikke.");

                return (false, "Du abonnerer allerede på denne politiker.");
            }

            return (true, "Abonnement oprettet.");
        }

        public async Task<(bool success, string message)> UnsubscribeAsync(
            int userId,
            int politicianTwitterId
        )
        {
            var success = await _repository.UnsubscribeAsync(userId, politicianTwitterId);
            return success ? (true, "Abonnement slettet.") : (false, "Abonnement ikke fundet.");
        }

        public async Task<(bool success, object? result, string? message)> LookupPoliticianAsync(
            int aktorId
        )
        {
            var result = await _repository.LookupPoliticianAsync(aktorId);

            if (result == null)
                return (
                    false,
                    null,
                    $"Ingen tilknyttet 'PoliticianTwitterId' fundet for Aktor ID {aktorId}. Er data linket i databasen?"
                );

            return (true, result, null);
        }
    }
}
