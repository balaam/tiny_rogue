using Unity.Entities;

namespace game
{
    // The Entity will attempt to move into Melee range to attack the player
    public struct MeleeAttackMovement : IComponentData
    {
        // ECS bug: somehow this breaks fog of war if there's absolutely no data on the component 
        public bool doesNothing;
    }
}