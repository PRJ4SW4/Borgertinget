using backend.Interfaces.Utility;
using System;

namespace backend.Services.Utility
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public DateOnly TodayUtc => DateOnly.FromDateTime(UtcNow);
    }
}