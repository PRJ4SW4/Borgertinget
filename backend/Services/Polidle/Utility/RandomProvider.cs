// Fil: Services/Utility/RandomProvider.cs
using backend.Interfaces.Utility;
using System;

namespace backend.Services.Utility
{
    public class RandomProvider : IRandomProvider
    {
        // Brug ThreadStatic for at gøre Random thread-safe hvis nødvendigt,
        // men for alm. web requests er en enkelt instans ofte ok.
        // [ThreadStatic]
        // private static Random? _localRandom;
        // private static Random Instance => _localRandom ??= new Random();

        // Simplere singleton approach (mindre robust ved høj concurrency)
         private static readonly Random _random = new Random();


        public int Next(int maxValue) => _random.Next(maxValue);
        public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);
        public double NextDouble() => _random.NextDouble();
    }
}