using Unity.Entities;
using Unity.Mathematics;

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
        public float AnimationTime;

        public float3 Initial;
        public float3 Destination;
        public bool Moving;
        public float MoveTime;
    }
}
