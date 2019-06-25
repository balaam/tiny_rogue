using Unity.Entities;

namespace game
{
    public struct AnimationSequence : IComponentData
    {
        public int PlayerId;
        public int MoveId;
        public int DirectionId;
    }
}
