using Unity.Entities;

namespace game
{
    public struct Floor : IComponentData 
    {
        public int TileOffset;
    }
}