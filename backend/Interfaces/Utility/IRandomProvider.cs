namespace backend.Interfaces.Utility
{
    public interface IRandomProvider
    {
        int Next(int maxValue);
        int Next(int minValue, int maxValue);
        double NextDouble();
    }
}
