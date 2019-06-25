using Unity.Mathematics;
using Unity.Tiny.Core2D;

namespace game
{
    public class TinyRogueConstants
    {
        public static float TileWidth => GlobalGraphicsSettings.ascii ? 0.09f : 1f;
        public static float TileHeight => GlobalGraphicsSettings.ascii ? 0.16f : 1f;
        public static float HalfTile = TileWidth/2;
        public static int StartPlayerHealth = 10;
        public static float3 OffViewport = new float3(-9999, -9999, 0);
        public static Color DefaultColor = new Color(168.0f/255.0f, 168.0f/255.0f,168.0f/255.0f);
    }
}