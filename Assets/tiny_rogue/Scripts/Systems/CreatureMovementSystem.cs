using Unity.Entities;

[UpdateBefore(typeof(TurnManagementSystem))]
public class CreatureMovementSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var tms = EntityManager.World.GetOrCreateSystem<TurnManagementSystem>();
        if (tms.NeedToTickTurn) // Don't always be moving!
        {
            // Find player to navigate towards
            int2 playerPos;
            Entities.WithAll<Player>().ForEach((Entity player, ref WorldCoord coord) =>
            {
                playerPos.x = coord.x;
                playerPos.y = coord.y;
            });

            // Move all creatures towards Player
            Entities.ForEach((Entity creature, WorldCoord coord, ref MeleeAttackMovement mov) =>
            {
                int2 creaturePos = new int2(coord.x, coord.y);
                int2 nextStep = AStarPathfinding.getNextStep(creaturePos, playerPos);
                if (nextStep == playerPos)
                {
                    // TODO deal damage to player
                }
                else
                {
                    // TODO currently all monsters are ghosts that travel through walls, this is mostly because the
                    // pathfinding function cannot identify walls so the alternative is monsters mashing their face against the wall
                    coord.x = nextStep.x;
                    coord.y = nextStep.y;
                }
            });
        }
    }
}
