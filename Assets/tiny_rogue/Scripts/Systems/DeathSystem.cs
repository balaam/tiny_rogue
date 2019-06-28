using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using UnityEngine;

namespace game
{

    [UpdateInGroup(typeof(TurnSystemGroup))]
    [UpdateAfter(typeof(ActionResolutionSystem))]
    [UpdateAfter(typeof(TrapSystem))]
    [UpdateAfter(typeof(HealthItemsSystem))]
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
                            var anim = EntityManager.World.GetExistingSystem<AnimationSystem>();

                            if (EntityManager.HasComponent(creature, typeof(Player)))
                            {
                                var player = EntityManager.GetComponentData<Player>(creature);
                                var sequencePlayer = EntityManager.GetComponentData<Sprite2DSequencePlayer>(creature);
                                
                                player.Dead = true;
                                sequencePlayer.loop = LoopMode.Once;
                                sequencePlayer.speed = 0.5f;
                                animated.AnimationTime = 10f;
                                animated.Action = Action.Die;
                                animated.AnimationTrigger = true;
                                
                                anim.SetAnimation(ref animated, ref sequencePlayer);
                                
                                EntityManager.SetComponentData(creature, player);
                                EntityManager.SetComponentData(creature, animated);
                                EntityManager.SetComponentData(creature, sequencePlayer);
                            }
                            else
                            {
                                Entity death = PostUpdateCommands.CreateEntity();
                                Parent parent = new Parent();
                                
                                Translation trans = pos;
                                Sprite2DRenderer deathRenderer = new Sprite2DRenderer { color = TinyRogueConstants.DefaultColor };
                                Sprite2DSequencePlayer deathPlayer = new Sprite2DSequencePlayer { speed = 0.5f };
                                Animated deathAnimated = new Animated { Id = animated.Id, Direction = animated.Direction, Action = Action.Die, AnimationTime = 0.75f, AnimationTrigger = true };
                                LayerSorting layerSorting = new LayerSorting { layer = 2 };
                                
                                anim.SetAnimation(ref deathAnimated, ref deathPlayer);

                                PostUpdateCommands.AddComponent(death, parent);
                                PostUpdateCommands.AddComponent(death, trans);
                                PostUpdateCommands.AddComponent(death, deathRenderer);
                                PostUpdateCommands.AddComponent(death, deathPlayer);
                                PostUpdateCommands.AddComponent(death, deathAnimated);
                                PostUpdateCommands.AddComponent(death, layerSorting);
                                
                                PostUpdateCommands.DestroyEntity(creature);
                            }
                        }
                    }
                });
            }
        }
    }

}
