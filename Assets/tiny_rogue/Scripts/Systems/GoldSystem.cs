using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace game
{
    [UpdateBefore(typeof(LogSystem))]
    public class GoldSystem : TurnSystem
    {
        protected override void OnUpdate()
        {
            // Did the player step on a coin?
            Entities.ForEach((Entity creature, ref WorldCoord coord, ref GoldCount gp) =>
            {
                int2 creaturePos = new int2(coord.x, coord.y);
                int amount = 0;
                Entities.WithAll<Gold>().ForEach((Entity coin, ref WorldCoord coinCoord) =>
                {
                    if (creaturePos.x == coinCoord.x && creaturePos.y == coinCoord.y)
                    {
                        if(EntityManager.HasComponent(creature, typeof(Player)))
                        {
                            var log = EntityManager.World.GetExistingSystem<LogSystem>();
                            log.AddLog("You collected 2 gold coins.");
                            var tms = EntityManager.World.GetExistingSystem<TurnManagementSystem>();
                        }

                        amount = 2;
                        PostUpdateCommands.DestroyEntity(coin);
                    }
                });
                gp.count = gp.count + amount;
            });
        }
    }
}
