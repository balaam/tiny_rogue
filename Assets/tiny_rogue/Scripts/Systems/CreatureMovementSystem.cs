using game;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace game
{
    [UpdateInGroup(typeof(TurnSystemGroup))]
    [UpdateBefore(typeof(ActionResolutionSystem))]
    public class CreatureMovementSystem : ComponentSystem
    {

        protected override void OnUpdate()
        {
            var tms = EntityManager.World.GetOrCreateSystem<TurnManagementSystem>();
            var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
            // Find player to navigate towards
            int2 playerPos = int2.zero;
            int viewDepth = 0;
            Entities.WithAll<Player>().ForEach((Entity player, ref WorldCoord coord, ref Animated animated, ref Sight sight) =>
            {
                playerPos.x = coord.x;
                playerPos.y = coord.y;
                viewDepth = sight.SightRadius;
            });

            // Move all creatures towards Player
            Entities.WithNone<PatrollingState>().WithAll<MeleeAttackMovement>()
                .ForEach((Entity creature, ref WorldCoord coord, ref Speed speed, ref Animated animated, ref TurnPriorityComponent pri) =>
                {
                    if (tms.TurnCount % speed.SpeedRate == 0)
                    {
                        int2 creaturePos = new int2(coord.x, coord.y);
                        int2 nextPos =
                            AStarPathfinding.getNextStep(creaturePos, playerPos, gss.View, EntityManager);
                        Action movement = getDirection(creaturePos, nextPos);
                        tms.AddActionRequest(movement, creature, coord, animated.Direction, pri.Value);
                    }
                });

            Entities.ForEach((Entity creature, ref WorldCoord coord, ref PatrollingState patrol, ref Speed speed, ref Animated animated, ref TurnPriorityComponent pri) =>
            {
                if (tms.TurnCount % speed.SpeedRate == 0)
                {
                    int2 monsterPos = new int2(coord.x, coord.y);
                    if (patrol.destination.Equals(new int2(0, 0)) || patrol.destination.Equals(monsterPos))
                    {
                        DungeonSystem ds = EntityManager.World.GetExistingSystem<DungeonSystem>();
                        patrol.destination = ds.GetRandomPositionInRandomRoom();
                    }

                    EntityManager.SetComponentData(creature, patrol);
                    // Follow defined path now that we have ensured that one exists
                    int2 nextPos =
                        AStarPathfinding.getNextStep(monsterPos, patrol.destination, gss.View, EntityManager);
                    Action movement = getDirection(monsterPos, nextPos);
                    tms.AddActionRequest(movement, creature, coord, animated.Direction, pri.Value);

                }
            });
            
            View view = gss.View;
            bool[] blockedPosition = new bool[view.Height * view.Width];
            Entities.WithAll<BlockMovement>().ForEach((Entity e, ref WorldCoord coord) =>
            {
                int i = View.XYToIndex(new int2(coord.x, coord.y), view.Width);
                blockedPosition[i] = true;
            });
            
            // Monsters stop Patrolling and start actively following the Player if they can spot the Player
            Entities.WithAll<PatrollingState>().ForEach(
                (Entity e, ref WorldCoord coord, ref Sight sight) =>
                {
                    int2 pos = new int2(coord.x, coord.y);
                    float totalDistance = math.sqrt(math.pow(math.distance(playerPos.x, pos.x), 2) +
                                                    math.pow(math.distance(playerPos.y, pos.y), 2));
                    
                    if (totalDistance <= viewDepth && !FogOfWarSystem.SightBlocked(playerPos, pos, view, blockedPosition))
                    {
                        PostUpdateCommands.RemoveComponent(e, typeof(PatrollingState));
                    }
                });
        }

        private static Action getDirection(int2 current, int2 target)
        {
            if (current.y < target.y)
            {
                return Action.MoveDown;
            }

            if (current.y > target.y)
            {
                return Action.MoveUp;
            }

            if (current.x > target.x)
            {
                return Action.MoveLeft;
            }

            if (current.x < target.x)
            {
                return Action.MoveRight;
            }

            return Action.None;
        }
    }
}