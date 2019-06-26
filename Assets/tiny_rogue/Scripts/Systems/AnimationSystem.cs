using System;
using game;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using UnityEngine;
using Action = game.Action;

public class AnimationSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        if (!SpriteSystem.Loaded) return;

        // Keep this exclusively for the graphical version
        if (!GlobalGraphicsSettings.ascii)
        {
            Entities.WithAll<NeedsAnimationStart>().ForEach((Entity e) =>
            {
                StartAnimation(e, Action.None, Direction.Right);
                PostUpdateCommands.RemoveComponent<NeedsAnimationStart>(e);
            });
            
            Entities.WithAll<Mobile>().ForEach((Entity e, ref Mobile mobile, ref Translation translation) =>
            {
                if (mobile.Moving)
                {
                    var frameTime = World.TinyEnvironment().fixedFrameDeltaTime;
                    // Double frame time, so the move takes 0.25 of a second
                    mobile.MoveTime += frameTime * 4;
                    translation.Value = math.lerp(mobile.Initial, mobile.Destination, mobile.MoveTime);
                    // Ensure player is left in correct position
                    if (mobile.MoveTime > 1f)
                    {
                        mobile.Moving = false;
                        translation.Value = mobile.Destination;
                        EntityManager.SetComponentData(e, new WorldCoord { x = mobile.DestPos.x, y = mobile.DestPos.y });
                    }
                }
            });
            
            Entities.WithAll<Animated, Sprite2DSequencePlayer>()
                .ForEach((Entity e, ref Animated animated, ref Sprite2DSequencePlayer sequencePlayer) =>
            {
                if (animated.AnimationTrigger)
                {
                    // Count down one-off animation/action
                    var frameTime = World.TinyEnvironment().fixedFrameDeltaTime;
                    animated.AnimationTime -= frameTime;

                    if (animated.AnimationTime <= 0f)
                    {
                        animated.AnimationTrigger = false;
                        animated.AnimationTime = 0f;
                        animated.Action = Action.None;
                        SetAnimation(ref animated, ref sequencePlayer);
                        Debug.Log("Switch animation");
                    }
                }
            });
        }
    }

    /// <summary>
    /// Start an animation for a given action.
    /// </summary>
    /// <param name="e">Entity to animate. Looks for (Sprite2DSequencePlayer AND Animated) and/or Mobile component(s).</param>
    /// <param name="action">Action to map to animation. Avoid sending None.</param>
    /// <param name="direction">Direction the action is in</param>
    public void StartAnimation(Entity e, Action action, Direction direction)
    {
        // Keep this exclusively for the graphical version
        if (!GlobalGraphicsSettings.ascii)
        {
            // Handle animated movement.
            // Don't show walking animation if character is moving towards wall.
            if (action == Action.Move)
            {
                if (!EntityManager.HasComponent<Mobile>(e))
                {
                    throw new Exception("This entity is not Mobile. It cannot move!");
                }
                
                // Start moving animation
                var mobile = EntityManager.GetComponentData<Mobile>(e);
                mobile.Moving = true;
                mobile.MoveTime = 0f;
                EntityManager.SetComponentData(e, mobile);
            }

            // Handle animated sprites.
            if (EntityManager.HasComponent<Animated>(e))
            {
                if (!EntityManager.HasComponent<Sprite2DSequencePlayer>(e))
                {
                    throw new Exception("Cannot animate without a Sprite2DSequencePlayer!");
                }
                
                var animated = EntityManager.GetComponentData<Animated>(e);
                var sequencePlayer = EntityManager.GetComponentData<Sprite2DSequencePlayer>(e);

                animated.Action = action;
                animated.Direction = direction;
                
                // Set animation
                if (animated.Action != Action.None)
                {
                    animated.AnimationTrigger = true;
                    animated.AnimationTime = 0.5f;
                }

                switch (animated.Action)
                {
                    case Action.None:
                    case Action.Wait:
                    case Action.Interact:
                        sequencePlayer.speed = 0.5f;
                        break;
                    case Action.Attack:
                        sequencePlayer.speed = 0.5f;
                        animated.AnimationTime = 0.25f;
                        break;
                    case Action.Move:
                        sequencePlayer.speed = 0.75f;
                        animated.AnimationTime = 0.25f;
                        break;
                }
                SetAnimation(ref animated, ref sequencePlayer);
                
                // Update components
                EntityManager.SetComponentData(e, animated);
                EntityManager.SetComponentData(e, sequencePlayer);
            }
        }
    }

    void SetAnimation(ref Animated animated, ref Sprite2DSequencePlayer sequencePlayer)
    {
        var direction = (int)animated.Direction;
        var action = (int)animated.Action;
        var id = animated.Id;
        Debug.Log($"Try set animation: {direction} {action}");

        Entity animation = Entity.Null;
        Entities.WithAll<AnimationSequence>().ForEach((Entity entity, ref AnimationSequence animationSequence) =>
        {
            if (animationSequence.MoveId == action && animationSequence.DirectionId == direction && animationSequence.PlayerId == id)
            {
                animation = entity;
                Debug.Log("Found animation");
            }
        });
        Debug.Log("Setting animation");
        sequencePlayer.time = 0f;
        sequencePlayer.sequence = animation;
    }
}
