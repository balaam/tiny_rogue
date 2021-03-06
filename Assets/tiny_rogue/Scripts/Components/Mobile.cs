using Unity.Entities;
using Unity.Mathematics;

namespace game
{
    public struct Mobile : IComponentData
    {
        public float3 Initial;
        public float3 Destination;
        public bool Moving;
        public float MoveTime;
    }
}
