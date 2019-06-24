using Unity.Mathematics;

namespace game
{
    public class TinyRogueConstants
    {
        public static float TileWidth = 1f;
		public static float TileHeight = 1f;
        public static float HalfTile = TileHeight/2;
        public static int StartPlayerHealth = 10;
        public static float3 OffViewport = new float3(-9999, -9999, 0);
    }
}