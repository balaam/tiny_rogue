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
            Entities.ForEach((ref Player player, ref Sprite2DSequencePlayer sequencePlayer, ref Translation translation) =>
            {
                if (player.Moving)
                {
                    var frameTime = World.TinyEnvironment().fixedFrameDeltaTime;
                    // Double frame time, so the move takes 0.5 of a second
                    player.MoveTime += frameTime * 2;
                    translation.Value = math.lerp(player.Initial, player.Destination, player.MoveTime);
                    // Ensure player is left in correct position
                    if (player.MoveTime > 1f)
                    {
                        player.Moving = false;
                        translation.Value = player.Destination;
                    }
                }
                
                if (player.AnimationTrigger)
                {
                    // Count down one-off animation/action
                    var frameTime = World.TinyEnvironment().fixedFrameDeltaTime;
                    player.AnimationTime -= frameTime;

                    if (player.AnimationTime <= 0f)
                    {
                        player.AnimationTrigger = false;
                        player.AnimationTime = 0f;
                        player.Action = Action.None;
                        SetAnimation(ref player, ref sequencePlayer);
                        Debug.Log("Switch animation");
                    }
                }
            });
        }
    }

    /// <summary>
    /// Start an animation for a given action.
    /// </summary>
    /// <param name="action">Action to map to animation. Avoid sending None.</param>
    /// <param name="moved">Whether or not the character can move.</param>
    public void StartAnimation(Action action, bool moved)
    {
        // Keep this exclusively for the graphical version
        if (!GlobalGraphicsSettings.ascii)
        {
            Entities.WithAll<Player>().ForEach((ref Player player, ref Sprite2DSequencePlayer sequencePlayer) =>
            {
                player.Action = action;
                
                // Map directional move to move and direction
                if (player.Action == Action.MoveLeft)
                {
                    player.Direction = Direction.West;
                    player.Action = Action.Move;
                }
                else if (player.Action == Action.MoveRight)
                {
                    player.Direction = Direction.East;
                    player.Action = Action.Move;
                }
                else if (player.Action == Action.MoveUp)
                {
                    player.Direction = Direction.East;
                    player.Action = Action.Move;
                }
                else if (player.Action == Action.MoveDown)
                {
                    player.Direction = Direction.West;
                    player.Action = Action.Move;
                }

                // Don't show walking animation if character is moving towards wall
                if (player.Action == Action.Move)
                {
                    if (!moved)
                    {
                        player.Action = Action.None;
                    }
                    else
                    {
                        player.Moving = true;
                        player.MoveTime = 0f;
                    }
                }
                
                Debug.Log($"Setting Action as {(int)player.Action}");

                if (player.Action != Action.None)
                {
                    player.AnimationTrigger = true;
                    player.AnimationTime = 0.5f;
                    sequencePlayer.speed = player.Action == Action.Move ? 0.75f : 0.5f;
                    SetAnimation(ref player, ref sequencePlayer);
                }
                else
                {
                    sequencePlayer.speed = 0.5f;
                    SetAnimation(ref player, ref sequencePlayer);
                }
            });
        }
    }

    public void SetAnimation(ref Player player, ref Sprite2DSequencePlayer sequencePlayer)
    {
        var direction = (int)player.Direction;
        var action = (int)player.Action;
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
