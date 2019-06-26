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
        public const float OffViewport = -9999;
        public const float DefaultColorBase = 168.0f/255.0f;

        public static Unity.Tiny.Core2D.Color DefaultColor =>
            GlobalGraphicsSettings.ascii
                ? new Unity.Tiny.Core2D.Color(TinyRogueConstants.DefaultColorBase, TinyRogueConstants.DefaultColorBase, TinyRogueConstants.DefaultColorBase)
                : Unity.Tiny.Core2D.Color.Default;
    }
}