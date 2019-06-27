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
                Entities.ForEach((Entity creature, ref HealthPoints hp, ref Sprite2DRenderer renderer, ref Translation pos, ref Animated animated) =>
                {
                    if (hp.now <= 0)
                    {
                        if (GlobalGraphicsSettings.ascii)
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
                        else
                        {
                            Entity death = PostUpdateCommands.CreateEntity();
                            Parent parent = new Parent();
                            Translation trans = pos;
                            Sprite2DRenderer deathRenderer = new Sprite2DRenderer { color = TinyRogueConstants.DefaultColor };
                            Sprite2DSequencePlayer deathPlayer = new Sprite2DSequencePlayer { speed = 0.5f };
                            Animated deathAnimated = new Animated { Id = animated.Id, Direction = Direction.Right, Action = Action.Die, AnimationTime = 0.5f, AnimationTrigger = true };
                            LayerSorting layerSorting = new LayerSorting { layer = 2 };
                            var anim = EntityManager.World.GetExistingSystem<AnimationSystem>();
                            anim.SetAnimation(ref deathAnimated, ref deathPlayer);
                            PostUpdateCommands.AddComponent(death, parent);
                            PostUpdateCommands.AddComponent(death, trans);
                            PostUpdateCommands.AddComponent(death, deathRenderer);
                            PostUpdateCommands.AddComponent(death, deathPlayer);
                            PostUpdateCommands.AddComponent(death, deathAnimated);
                            PostUpdateCommands.AddComponent(death, layerSorting);
                            
                            if (EntityManager.HasComponent(creature, typeof(Player)))
                            {
                                renderer.color.a = 0f;
                            }
                            else
                            {
                                PostUpdateCommands.DestroyEntity(creature);
                            }
                        }
                    }
                });
            }
        }
    }

}
