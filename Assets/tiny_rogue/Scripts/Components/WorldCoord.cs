using Unity.Entities;
using Unity.Tiny;
using Unity.Tiny.Core2D;

namespace game
{
        public struct WorldCoord : IComponentData
        {
                public int x;
                public int y;
        }
}