using Unity.Entities;
using Unity.Tiny;
using Unity.Tiny.Core2D;

namespace game
{
        public struct MapData : IComponentData
        {
                public int width;
                public int height;
        }
}