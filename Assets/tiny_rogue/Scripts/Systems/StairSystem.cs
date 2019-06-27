using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace game
{
    // Notes:
    // - 
    public class StairSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // Did the player step on a coin?
            Entities.WithAll<Player>().ForEach((Entity player, ref WorldCoord worldCoord) =>
            {
                int2 playerPos = new int2(worldCoord.x, worldCoord.y);

                Entities.WithAll<Stairway>().ForEach((Entity stair, ref WorldCoord stairCoord) =>
                {
                    if (playerPos.x == stairCoord.x && playerPos.y == stairCoord.y)
                    {
                        
                        if(EntityManager.HasComponent(player, typeof(Player)))
                        {
                            var log = EntityManager.World.GetExistingSystem<LogSystem>();
                            log.AddLog("You found the Stairs. Press Z to descend.");
                            var tms = EntityManager.World.GetExistingSystem<TurnManagementSystem>();
                            tms.NeedToTickTurn = true;
                            
                        }
                    }
                });
            });
        }
    }
}
