using Unity.Mathematics;
using Unity.Tiny.Core2D;

namespace game
{
    public class TinyRogueConstants
    {
        public static float TileWidth = 0.09f;
		public static float TileHeight = 0.16f;
        public static float HalfTile = TileHeight/2;
        public static int StartPlayerHealth = 10;
        public static float3 OffViewport = new float3(-9999, -9999, 0);
        public static Color DefaultColor = new Color(168.0f/255.0f, 168.0f/255.0f,168.0f/255.0f);
    }
}