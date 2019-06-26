using Unity.Entities;

namespace game
{
    public struct Door : IComponentData
    {
        public bool Opened;
        public bool Locked;
        public bool Hidden;
        public bool Horizontal;
    }
}
