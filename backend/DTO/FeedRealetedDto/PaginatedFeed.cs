namespace backend.DTOs
{ // dene dto DTO er til at hente tweets fra databasen og vise dem i en pagineret feed
    public class PaginatedFeedResult
    {
        public List<TweetDto> Tweets { get; set; } = new List<TweetDto>();
        public bool HasMore { get; set; }

        public List<PollDetailsDto> LatestPolls { get; set; } = new List<PollDetailsDto>();
    }
}
