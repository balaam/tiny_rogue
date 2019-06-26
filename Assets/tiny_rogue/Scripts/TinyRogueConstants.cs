using Unity.Mathematics;
using Unity.Tiny.Core2D;

namespace game
{
    public class TinyRogueConstants
    {
        /// <summary>
        /// 1/DoorProbability or being drawn for each door in the level.
        /// </summary>
        public const int DoorProbability = 5;
        public const int StartPlayerHealth = 10;
        public static float3 OffViewport = new float3(-9999, -9999, 0);
        public static Color DefaultColor = new Color(168.0f/255.0f, 168.0f/255.0f,168.0f/255.0f);
    }
}