using Unity.Entities;

namespace game
{
    public enum Direction
    {
        Right = 0,
        Down = 1,
        Left = 2,
        Up = 3
    }
    
    public struct Animated : IComponentData
    {
        public int Id;
        public Direction Direction;
        public Action Action;
        public bool AnimationTrigger;
        public float AnimationTime;
    }
}
