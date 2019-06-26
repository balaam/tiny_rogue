using Unity.Entities;

namespace game
{
    public struct HealthBonus: IComponentData
    {
        public int healthAdded;
    }
}