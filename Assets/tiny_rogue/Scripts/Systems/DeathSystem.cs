using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using UnityEngine;

namespace game
{
    public class DeathSystem : ComponentSystem
    {
        protected override void OnUpdate() { }

        public void OnUpdateManual()
        {
            Entities.ForEach((Entity creature, ref HealthPoints hp, ref Translation pos) =>
            {
                if (hp.now <= 0)
                {
                    if(EntityManager.HasComponent(creature, typeof(Player)))
                    {
                        var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
                        gss.MoveToGameOver();
                        // Move the character off screen.
                        pos.Value = new float3(-9999, -9999, 0);
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
