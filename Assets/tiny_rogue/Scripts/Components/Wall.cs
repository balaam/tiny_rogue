using Unity.Entities;

namespace game
{
    public struct Wall : IComponentData
    {
        public int TileOffset;
    }
}