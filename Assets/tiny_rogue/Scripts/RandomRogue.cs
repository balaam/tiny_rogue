using Unity.Entities;
using Unity.Mathematics;

public class RandomRogue
{
    private static Random random;
    private static bool initialized = false;

    public static int Next()
    {
        if (!initialized)
            throw new System.InvalidOperationException("RandomRogue used before Init");

        return random.NextInt();
    }

    public static int Next(int maxValue)
    {
        if (!initialized)
            throw new System.InvalidOperationException("RandomRogue used before Init");

        return random.NextInt(maxValue);
    }

    public static int Next(int minValue, int maxValue)
    {
        if (!initialized)
            throw new System.InvalidOperationException("RandomRogue used before Init");

        return random.NextInt(minValue, maxValue);
    }

    public static void Init(uint i)
    {
        random = new Random(i);
        random.InitState();

        initialized = true;
    }
}
