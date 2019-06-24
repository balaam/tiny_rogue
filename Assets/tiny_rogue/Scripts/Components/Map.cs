using Unity.Entities;
using Unity.Tiny;
using Unity.Tiny.Core2D;

namespace game
{
        public struct Map : IComponentData
        {
                public int width;
                public int height;
        }
}