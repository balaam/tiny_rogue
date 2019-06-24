using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace game
{
    // Notes:
    // - 
    public class GoldSystem : ComponentSystem
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
                            var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
                            gss.TurnManager.NeedToTickTurn = true;
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
