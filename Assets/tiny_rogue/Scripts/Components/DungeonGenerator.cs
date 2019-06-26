using Unity.Entities;

namespace game
{
    public struct DungeonGenerator : IComponentData
    {
        public int MaxNumberOfRooms;
        public int MinRoomSize;
        public int MaxRoomSize;
        public int NumberOfCollectibles;
    }
}