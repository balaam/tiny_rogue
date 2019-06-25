using Unity.Entities;

namespace game
{
    public struct ActionStream : IBufferElementData
    {
        public Action action;
        public float time;
    }
}