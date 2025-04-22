using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class SubscriptionService
    {
        private readonly DataContext _context;

        public SubscriptionService(DataContext context)
        {
            _context = context;
        }

        public void Subscribe(User user, PoliticianTwitterId politician)
        {
            // Hook up event handler
            politician.TweetPosted += user.OnTweetPosted;

            // Add subscription to Db if not already added
            bool alreadySubscribed = _context.Subscriptions.Any(s =>
                s.UserId == user.Id && s.PoliticianTwitterId == politician.Id
            );

            if (!alreadySubscribed)
            {
                _context.Add(
                    new Subscription { UserId = user.Id, PoliticianTwitterId = politician.Id }
                );
                _context.SaveChanges();
            }
        }

        public void Unsubscribe(User user, PoliticianTwitterId politician)
        {
            politician.TweetPosted -= user.OnTweetPosted;

            var subscription = _context.Subscriptions.FirstOrDefault(s =>
                s.UserId == user.Id && s.PoliticianTwitterId == politician.Id
            );

            if (subscription != null)
            {
                _context.Subscriptions.Remove(subscription);
                _context.SaveChanges();
            }
        }

        public void InitializeUserSubscriptions(User user)
        {
            var politicians = _context
                .Subscriptions.Include(s => s.Politician)
                .Where(s => s.UserId == user.Id)
                .Select(s => s.Politician)
                .ToList();

            foreach (var politician in politicians)
            {
                politician.TweetPosted += user.OnTweetPosted;
            }
        }
    }
}
