using Unity.Entities;
using Unity.Mathematics;

public class RandomRogue
{
    private static Random random;
    private static bool initialized = false;

    public static int Next()
    {
        if (!initialized)
            Init();

        return random.NextInt();
    }

    public static int Next(int maxValue)
    {
        if (!initialized)
            Init();

        return random.NextInt(maxValue);
    }

    public static int Next(int minValue, int maxValue)
    {
        if (!initialized)
            Init();

        return random.NextInt(minValue, maxValue);
    }

    private static void Init()
    {
        long seed = 929374892134987;
        random = new Random((uint)seed);
        random.InitState();

        initialized = true;
    }
}
