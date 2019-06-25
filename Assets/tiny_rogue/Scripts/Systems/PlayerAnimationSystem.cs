using game;
using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using UnityEngine;

public class PlayerAnimationSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        if (!SpriteSystem.Loaded) return;

        // Keep this exclusively for the graphical version
        if (!GlobalGraphicsSettings.ascii)
        {
            Debug.Log("Animation Update");
            Entities.WithAll<Player>().ForEach((ref Player player, ref Sprite2DSequencePlayer sequencePlayer) =>
            {
                if (player.AnimationTrigger)
                {
                    // Count down one-off animation/action
                    var frameTime = World.TinyEnvironment().frameTime;
                    player.AnimationTime -= frameTime;

                    if (player.AnimationTime <= 0f)
                    {
                        player.AnimationTrigger = false;
                        player.AnimationTime = 0f;
                        player.Action = Action.None;
                        //sequencePlayer.sequence = 
                        SetAnimation(ref player, ref sequencePlayer);
                    }
                }
            });
        }
    }

    public void StartAnimation(Action action)
    {
        // Keep this exclusively for the graphical version
        if (!GlobalGraphicsSettings.ascii)
        {
            Entities.WithAll<Player>().ForEach((ref Player player, ref Sprite2DSequencePlayer sequencePlayer) =>
            {
                // If already doing an action, don't switch
                // TODO: throw an error here
                if (player.Action != Action.None) return;

                player.Action = action;

                if (player.Action != Action.None)
                {
                    player.AnimationTrigger = true;
                    player.AnimationTime = 1f;
                    sequencePlayer.speed = 1f;
                }
                else
                {
                    sequencePlayer.speed = 0.5f;
                }
            });
        }
    }

    public void SetAnimation(ref Player player, ref Sprite2DSequencePlayer sequencePlayer)
    {
        var direction = (int)player.Direction;
        var action = (int)player.Action;

        Entity animation = Entity.Null;
        Entities.WithAll<AnimationSequence>().ForEach((Entity entity, ref AnimationSequence animationSequence) =>
        {
            if (animationSequence.MoveId == action && animationSequence.DirectionId == direction && animationSequence.PlayerId == 0)
            {
                animation = entity;
            }
        });
        Debug.Log("Setting animation");
        sequencePlayer.sequence = animation;
    }
}
