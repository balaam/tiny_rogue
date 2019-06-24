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
