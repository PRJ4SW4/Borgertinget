using backend.Interfaces.Utility;

namespace backend.Services.Utility
{
    public class RandomProvider : IRandomProvider
    {
        private static readonly Random _random = new Random();

        public int Next(int maxValue) => _random.Next(maxValue);

        public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

        public double NextDouble() => _random.NextDouble();
    }
}
