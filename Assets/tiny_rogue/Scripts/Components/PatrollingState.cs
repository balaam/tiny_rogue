using Unity.Entities;
using Unity.Mathematics;

namespace game
{
    // Creatures patrol the level when they have not yet spotted the player
    // Since patrolling isn't implemented yet this is synonymous with "inactive creature"
    // Note that Patrolling Creatures without Sight will not activate at all!
    // This is fine but all creatures should activate once hit.
    public struct PatrollingState: IComponentData
    {
        // Efficient cache of destination path
//        public SavedPath currentPath;

        // Inefficient save of destination
        public int2 destination;
    }
}