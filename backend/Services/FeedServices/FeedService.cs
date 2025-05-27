using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Models;
using backend.Repositories.Feed;
using Microsoft.Extensions.Logging;

namespace backend.Services.Feed
{
    public class FeedService : IFeedService
    {
        private readonly IFeedRepository _feedRepository;
        private readonly ILogger<FeedService> _logger;

        public FeedService(IFeedRepository feedRepository, ILogger<FeedService> logger)
        {
            _feedRepository =
                feedRepository ?? throw new ArgumentNullException(nameof(feedRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<PoliticianInfoDto>> GetUserSubscriptionsAsync(int userId)
        {
            try
            {
                return await _feedRepository.GetUserSubscriptionsAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting subscriptions for user {userId}");
                throw;
            }
        }

        public async Task<PaginatedFeedResult> GetUserFeedAsync(
            int userId,
            int page,
            int pageSize,
            int? politicianId
        )
        {
            try
            {
                if (page < 1)
                    page = 1;
                if (pageSize < 1)
                    pageSize = 5;
                if (pageSize > 50)
                    pageSize = 50;

                List<int> relevantPoliticianIds;
                bool isFiltered = politicianId.HasValue;

                if (isFiltered)
                {
                    int politicianIdValue = politicianId!.Value;

                    bool isSubscribed = await _feedRepository.IsUserSubscribedToPoliticianAsync(
                        userId,
                        politicianIdValue
                    );
                    if (!isSubscribed)
                        return new PaginatedFeedResult();

                    relevantPoliticianIds = new List<int> { politicianIdValue };
                }
                else
                {
                    relevantPoliticianIds =
                        await _feedRepository.GetUserSubscribedPoliticianIdsAsync(userId);
                }

                if (!relevantPoliticianIds.Any())
                {
                    return new PaginatedFeedResult();
                }

                List<Tweet> tweetsToPaginate;
                if (isFiltered)
                {
                    tweetsToPaginate = await _feedRepository.GetTweetsByPoliticianIdAsync(
                        politicianId!.Value
                    );
                }
                else
                {
                    tweetsToPaginate = await _feedRepository.GetTopTweetsByPoliticianIdsAsync(
                        relevantPoliticianIds,
                        5
                    );
                }

                int totalTweets = tweetsToPaginate.Count;
                int skipAmountTweets = (page - 1) * pageSize;
                var pagedTweets = tweetsToPaginate.Skip(skipAmountTweets).Take(pageSize).ToList();
                bool hasMoreTweets = skipAmountTweets + pagedTweets.Count < totalTweets;

                var feedTweetDtos = pagedTweets
                    .Select(t => new TweetDto
                    {
                        TwitterTweetId = t.TwitterTweetId,
                        Text = t.Text,
                        ImageUrl = t.ImageUrl,
                        Likes = t.Likes,
                        Retweets = t.Retweets,
                        Replies = t.Replies,
                        CreatedAt = t.CreatedAt,
                        AuthorName = t.Politician?.Name ?? "Ukendt",
                        AuthorHandle = t.Politician?.TwitterHandle ?? "ukendt",
                    })
                    .ToList();

                List<PollDetailsDto> latestPollDtos = new List<PollDetailsDto>();

                if (!isFiltered)
                {
                    var allLatestPolls = await _feedRepository.GetLatestPollsByPoliticianIdsAsync(
                        relevantPoliticianIds,
                        2
                    );

                    if (allLatestPolls.Any())
                    {
                        var pollIdsToCheck = allLatestPolls.Select(p => p.Id).Distinct().ToList();
                        var userVotesForLatestPolls =
                            await _feedRepository.GetUserVotesForPollsAsync(userId, pollIdsToCheck);

                        latestPollDtos = allLatestPolls
                            .OrderByDescending(p => p.CreatedAt)
                            .Select(p =>
                                MapPollToDetailsDto(
                                    p,
                                    p.Politician,
                                    userVotesForLatestPolls.ContainsKey(p.Id)
                                        ? userVotesForLatestPolls[p.Id]
                                        : null
                                )
                            )
                            .ToList();
                    }
                }

                return new PaginatedFeedResult
                {
                    Tweets = feedTweetDtos,
                    HasMore = hasMoreTweets,
                    LatestPolls = latestPollDtos,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Error getting feed for user {userId} with filter {politicianId}"
                );
                throw;
            }
        }

        private PollDetailsDto MapPollToDetailsDto(
            Poll poll,
            PoliticianTwitterId politician,
            UserVote? userVote
        )
        {
            int totalVotes = poll.Options?.Sum(o => o.Votes) ?? 0;
            return new PollDetailsDto
            {
                Id = poll.Id,
                Question = poll.Question,
                CreatedAt = poll.CreatedAt,
                EndedAt = poll.EndedAt,
                PoliticianId = politician.Id,
                PoliticianName = politician.Name,
                PoliticianHandle = politician.TwitterHandle,
                Options =
                    poll.Options?.Select(o => new PollOptionDto
                        {
                            Id = o.Id,
                            OptionText = o.OptionText,
                            Votes = o.Votes,
                        })
                        .OrderBy(o => o.Id)
                        .ToList() ?? new List<PollOptionDto>(),
                CurrentUserVoteOptionId = userVote?.ChosenOptionId,
                TotalVotes = totalVotes,
            };
        }
    }
}
