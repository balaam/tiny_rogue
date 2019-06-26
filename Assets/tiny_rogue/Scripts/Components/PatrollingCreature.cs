using Unity.Entities;

namespace game
{
    // Creatures patrol the level when they have not yet spotted the player
    // Since patrolling isn't implemented yet this is synonymous with "inactive creature"
    public struct PatrollingCreature: IComponentData
    {
        // TODO figure out how to enable this
//        public SavedPath currentPath;
    }
}