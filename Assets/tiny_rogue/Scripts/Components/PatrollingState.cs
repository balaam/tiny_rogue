using Unity.Entities;

namespace game
{
    // Creatures patrol the level when they have not yet spotted the player
    // Since patrolling isn't implemented yet this is synonymous with "inactive creature"
    // Note that Patrolling Creatures without Sight will not activate at all!
    // This is fine but all creatures should activate once hit.
    public struct PatrollingState: IComponentData
    {
//        public SavedPath currentPath;
    }
}