using Unity.Entities;

namespace game
{
    public struct Tile : IComponentData
    {
        public bool HasBeenRevealed;
        public bool IsSeen;
    }
}
