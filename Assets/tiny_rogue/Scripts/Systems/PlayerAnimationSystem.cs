using System;
using game;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using UnityEngine;
using Action = game.Action;

public class PlayerAnimationSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        if (!SpriteSystem.Loaded) return;

        // Keep this exclusively for the graphical version
        if (!GlobalGraphicsSettings.ascii)
        {
            Entities.WithAll<Mobile>().ForEach((Entity e, ref Mobile mobile, ref Translation translation) =>
            {
                if (mobile.Moving)
                {
                    var frameTime = World.TinyEnvironment().fixedFrameDeltaTime;
                    // Double frame time, so the move takes 0.5 of a second
                    mobile.MoveTime += frameTime * 2;
                    translation.Value = math.lerp(mobile.Initial, mobile.Destination, mobile.MoveTime);
                    // Ensure player is left in correct position
                    if (mobile.MoveTime > 1f)
                    {
                        mobile.Moving = false;
                        translation.Value = mobile.Destination;
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
    /// <param name="e">Entity to animate. Must have Mobile, Sprite2DSequencePlayer, and Animated components.</param>
    /// <param name="action">Action to map to animation. Avoid sending None.</param>
    /// <param name="moved">Whether or not the character can move.</param>
    public void StartAnimation(Entity e, Action action, bool moved)
    {
        // Keep this exclusively for the graphical version
        if (!GlobalGraphicsSettings.ascii)
        {
            // TODO: make mobile optional
            Animated animated = EntityManager.GetComponentData<Animated>(e);
            Mobile mobile = EntityManager.GetComponentData<Mobile>(e);
            Sprite2DSequencePlayer sequencePlayer = EntityManager.GetComponentData<Sprite2DSequencePlayer>(e);
                
            // Map directional move to move and direction
            if (action == Action.MoveLeft)
            {
                animated.Direction = Direction.Left;
                animated.Action = Action.Move;
            }
            else if (action == Action.MoveRight)
            {
                animated.Direction = Direction.Right;
                animated.Action = Action.Move;
            }
            else if (action == Action.MoveUp)
            {
                animated.Direction = Direction.Right;
                animated.Action = Action.Move;
            }
            else if (action == Action.MoveDown)
            {
                animated.Direction = Direction.Left;
                animated.Action = Action.Move;
            }

            // Don't show walking animation if character is moving towards wall
            if (animated.Action == Action.Move)
            {
                if (!moved)
                {
                    animated.Action = Action.None;
                }
                else
                {
                    mobile.Moving = true;
                    mobile.MoveTime = 0f;
                }
            }
            
            Debug.Log($"Setting Action as {(int)animated.Action}");

            if (animated.Action != Action.None)
            {
                animated.AnimationTrigger = true;
                animated.AnimationTime = 0.5f;
                sequencePlayer.speed = animated.Action == Action.Move ? 0.75f : 0.5f;
                SetAnimation(ref animated, ref sequencePlayer);
            }
            else
            {
                sequencePlayer.speed = 0.5f;
                SetAnimation(ref animated, ref sequencePlayer);
            }
            
            // Update components
            EntityManager.SetComponentData(e, animated);
            EntityManager.SetComponentData(e, mobile);
            EntityManager.SetComponentData(e, sequencePlayer);
        }
    }

    public void SetAnimation(ref Animated animated, ref Sprite2DSequencePlayer sequencePlayer)
    {
        var direction = (int)animated.Direction;
        var action = (int)animated.Action;
        Debug.Log($"Try set animation: {direction} {action}");

        Entity animation = Entity.Null;
        Entities.WithAll<AnimationSequence>().ForEach((Entity entity, ref AnimationSequence animationSequence) =>
        {
            if (animationSequence.MoveId == action && animationSequence.DirectionId == direction && animationSequence.PlayerId == 0)
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
