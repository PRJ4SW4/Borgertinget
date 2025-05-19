using System;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
                var politicianExists = await _context.PoliticianTwitterIds.AnyAsync(p =>
                    p.Id == politicianTwitterId
                );
                if (!politicianExists)
                {
                    return false;
                }

                bool alreadySubscribed = await _context.Subscriptions.AnyAsync(s =>
                    s.UserId == userId && s.PoliticianTwitterId == politicianTwitterId
                );

                if (alreadySubscribed)
                {
                    return false;
                }

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

        public async Task<object?> LookupPoliticianAsync(int aktorId)
        {
            try
            {
                return await _context
                    .PoliticianTwitterIds.AsNoTracking()
                    .Where(p => p.AktorId == aktorId)
                    .Select(p => new { politicianTwitterId = p.Id })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error looking up politician for aktor ID {aktorId}");
                return null;
            }
        }
    }
}
