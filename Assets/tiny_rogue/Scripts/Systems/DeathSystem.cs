using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using UnityEngine;

namespace game
{
    
    [UpdateAfter(typeof(TurnSystemGroup))]
    public class DeathSystem : ComponentSystem
    {
        protected override void OnUpdate() 
        { 
            var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
            if (gss.IsInGame)
            {
                Entities.ForEach((Entity creature, ref HealthPoints hp, ref Translation pos) =>
                {
                    if (hp.now <= 0)
                    {
                        if (EntityManager.HasComponent(creature, typeof(Player)))
                        {
                            gss.MoveToGameOver(PostUpdateCommands);
                            pos.Value = TinyRogueConstants.OffViewport;
                        }
                        else
                        {
                            PostUpdateCommands.DestroyEntity(creature);
                        }
                    }
                });
            }
        }
    }

}
