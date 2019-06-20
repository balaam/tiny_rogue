using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace game
{
    // Notes:
    // - This could be more less specific - i.e. it only handles spear traps what about other traps?
    // - The for loop nesting should be reversed. There are probably less traps than living things generally?
    public class TrapSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // Did anything with health step on a trap
            Entities.ForEach((Entity creature, ref WorldCoord coord, ref HealthPoints hp) =>
            {
                int2 creaturePos = new int2(coord.x, coord.y);
                int dmg = 0;
                Entities.WithAll<SpearTrap>().ForEach((Entity trap, ref WorldCoord trapCoord) =>
                {
                    if (creaturePos.x == trapCoord.x && creaturePos.y == trapCoord.y)
                    {
                        if(EntityManager.HasComponent(creature, typeof(Player)))
                        {
                            var log = EntityManager.World.GetExistingSystem<LogSystem>();
                            log.AddLog("A spear trap hits you for 5 damage.");
                            var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
                            gss.TurnManager.NeedToTickTurn = true;
                        }

                        dmg = 5;
                        PostUpdateCommands.DestroyEntity(trap);
                    }
                });
                hp.now = hp.now - dmg;
            });
        }
    }
}
