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
        private uint lastTurn = 0;

        protected override void OnUpdate()
        {
            var tms = EntityManager.World.GetOrCreateSystem<TurnManagementSystem>();
            if (lastTurn != tms.TurnCount) // Don't always be moving!
            {
                var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
                lastTurn = tms.TurnCount;
                // Find player to navigate towards
                int2 playerPos = int2.zero;
                Entities.WithAll<Player>().ForEach((Entity player, ref WorldCoord coord, ref Animated animated) =>
                {
                    playerPos.x = coord.x;
                    playerPos.y = coord.y;
                });

                // Move all creatures towards Player
                Entities.WithNone<PatrollingState>().WithAll<MeleeAttackMovement>()
                    .ForEach((Entity creature, ref WorldCoord coord, ref Speed speed, ref Animated animated) =>
                    {
                        if (lastTurn % speed.SpeedRate == 0)
                        {
                            int2 creaturePos = new int2(coord.x, coord.y);
                            int2 nextPos =
                                AStarPathfinding.getNextStep(creaturePos, playerPos, gss.View, EntityManager);
                            Action movement = getDirection(creaturePos, nextPos);
                            tms.AddDelayedAction(movement, creature, coord, animated.Direction);
                        }
                    });

                Entities.ForEach(
                    (Entity creature, ref WorldCoord coord, ref PatrollingState patrol, ref Speed speed,
                        ref Animated animated) =>
                    {
                        if (lastTurn % speed.SpeedRate == 0)
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
                            tms.AddDelayedAction(movement, creature, coord, animated.Direction);
                        }
                    });
            }
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