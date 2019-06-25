using Unity.Entities;

namespace game
{
    public enum Direction
    {
        East = 0,
        South = 1,
        West = 2,
        North = 3
    }
    
    public struct Player : IComponentData
    {
        public Direction Direction;
        public Action Action;
        public bool AnimationTrigger;
        public double AnimationTime;
    }
}
