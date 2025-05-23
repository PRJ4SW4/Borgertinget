using System;

namespace backend.Interfaces.Utility
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
        DateOnly TodayUtc { get; }
    }
}
