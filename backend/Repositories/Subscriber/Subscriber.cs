using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories.Subscription
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly DataContext _context;
        private readonly ILogger<SubscriptionRepository> _logger;

        public SubscriptionRepository(DataContext context, ILogger<SubscriptionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> SubscribeAsync(int userId, int politicianTwitterId)
        {
            try
            {
                var newSubscription = new Models.Subscription
                {
                    UserId = userId,
                    PoliticianTwitterId = politicianTwitterId,
                };

                _context.Subscriptions.Add(newSubscription);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Error subscribing user {userId} to politician {politicianTwitterId}"
                );
                return false;
            }
        }

        public async Task<bool> UnsubscribeAsync(int userId, int politicianTwitterId)
        {
            try
            {
                var subscription = await _context.Subscriptions.FirstOrDefaultAsync(s =>
                    s.UserId == userId && s.PoliticianTwitterId == politicianTwitterId
                );

                if (subscription == null)
                {
                    return false;
                }

                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Error unsubscribing user {userId} from politician {politicianTwitterId}"
                );
                return false;
            }
        }

        public async Task<PoliticianTwitterId?> LookupPoliticianAsync(int aktorId)
        {
            try
            {
                return await _context
                    .PoliticianTwitterIds.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.AktorId == aktorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error looking up politician for aktor ID {aktorId}");
                return null;
            }
        }
    }
}
