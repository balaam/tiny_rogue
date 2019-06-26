using game;
using Unity.Entities;
using Unity.Mathematics;

[UpdateBefore(typeof(TurnManagementSystem))]
public class CreatureMovementSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var tms = EntityManager.World.GetOrCreateSystem<TurnManagementSystem>();
        if (tms.NeedToTickTurn) // Don't always be moving!
        {
            // Find player to navigate towards
            int2 playerPos = int2.zero;
            Entities.WithAll<Player>().ForEach((Entity player, ref WorldCoord coord) =>
            {
                playerPos.x = coord.x;
                playerPos.y = coord.y;
            });

            // Move all creatures towards Player
            Entities.WithNone<PatrollingState>().WithAll<MeleeAttackMovement>()
                .ForEach((Entity creature, ref WorldCoord coord) =>
                {
                    int2 creaturePos = new int2(coord.x, coord.y);
                    int2 nextStep = AStarPathfinding.getNextStep(creaturePos, playerPos);
                    if (math.all(creaturePos == playerPos))
                    {
                        // TODO deal damage to player
                    }
                    else
                    {
                        // TODO currently all monsters are ghosts that travel through walls, this is mostly because the
                        // pathfinding function cannot identify walls so the alternative is monsters mashing their face against the wall
                        // TODO OH NO!! Need to get tileCoord out somehow
                        // TODO should be animated too
//                    EntityManager.SetComponentData(creature, nextStep);
                    }
                });
            
            Entities.WithAll<PatrollingState>().ForEach((Entity creature, ref WorldCoord coord, ref PatrollingState patrol) =>
            {
//                int2 monsterPos = new int2(coord.x, coord.y);
//                if (!patrol.currentPath.pathSteps.IsCreated || patrol.currentPath.pathSteps.Length == 0 ||
//                    patrol.currentPath.currentIdx == patrol.currentPath.pathSteps.Length)
//                {
//                    // Get new patrol path if one does not exist or has been completed
//                    DungeonSystem ds = EntityManager.World.GetExistingSystem<DungeonSystem>();
//                    int2 destination = ds.GetRandomPositionInRandomRoom();
//                    var newPath = AStarPathfinding.getPath(monsterPos, destination);
//                    // TODO set in new path, next line probably will not work
//                    patrol.currentPath = newPath;
//                }
//                
//                // Follow defined path now that we have ensured that one exists
//                int2 nextPos = AStarPathfinding.stepAlong(patrol.currentPath, monsterPos);
//                // TODO May now need to update the Monster path...
//                // TODO move monster
            });
        }
    }
}