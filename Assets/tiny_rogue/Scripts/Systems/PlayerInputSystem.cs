using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Tiny.Core2D;
using UnityEngine;
using KeyCode = Unity.Tiny.Input.KeyCode;
#if !UNITY_WEBGL
using InputSystem = Unity.Tiny.GLFW.GLFWInputSystem;
#else
    using InputSystem =  Unity.Tiny.HTML.HTMLInputSystem;
#endif

namespace game
{
    [UpdateAfter(typeof(StatusBarSystem))]
    public class PlayerInputSystem : ComponentSystem
    {
        private bool Replaying = false;
        private float StartTime;

        private bool alternateAction = false;

        public void StartRecording()
        {
            Replaying = false;
            StartTime = Time.time;

            // Reset all ActionStream buffers
            Entities.WithAll<ActionStream>().ForEach(e =>
            {
                var stream = EntityManager.GetBuffer<ActionStream>(e);
                stream.Clear();
            });
        }

        public void StartReplaying()
        {
            Replaying = true;
            StartTime = Time.time;
        }

        private Action GetActionFromInput()

        {
            var input = EntityManager.World.GetExistingSystem<InputSystem>();

            if (input.GetKey(KeyCode.LeftControl))
                alternateAction = true;
            else
                alternateAction = false;

            if (input.GetKeyDown(KeyCode.W) || input.GetKeyDown(KeyCode.UpArrow))
                return Action.MoveUp;
            if(input.GetKeyDown(KeyCode.S) || input.GetKeyDown(KeyCode.DownArrow))
                return Action.MoveDown;
            if(input.GetKeyDown(KeyCode.D) || input.GetKeyDown(KeyCode.RightArrow))
                return Action.MoveRight;
            if(input.GetKeyDown(KeyCode.A) || input.GetKeyDown(KeyCode.LeftArrow))
                return Action.MoveLeft;
            if (input.GetKeyDown(KeyCode.Z))
                return Action.Interact;
            if (input.GetKeyDown(KeyCode.Space))
                return Action.Wait;



            return Action.None;
        }

        private Action GetActionFromActionStream(Entity e, float time)
        {
            // Don't run if we don't have an action stream
            if (!EntityManager.HasComponent<ActionStream>(e))
                return Action.None;

            // Get the action buffer
            var stream = EntityManager.GetBuffer<ActionStream>(e);
            if (stream.Length <= 0)
                return Action.None;

            var action = stream[0];

            // Don't run if we've not reached the right time yet
            if (time < action.time)
                return Action.None;

            // Remove and run the action
            stream.RemoveAt(0);
            return action.action;

        }

        private WorldCoord GetMove(Action a)
        {
            WorldCoord c = new WorldCoord();
            switch (a)
            {
                case Action.MoveUp:
                    c.y -= 1;
                    break;
                case Action.MoveDown:
                    c.y += 1;
                    break;
                case Action.MoveRight:
                    c.x += 1;
                    break;
                case Action.MoveLeft:
                    c.x -= 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(a));
            }

            return c;
        }

        private Action GetAction(Entity e, float time)
        {
            return Replaying ? GetActionFromActionStream(e,time) : GetActionFromInput();
        }

        protected override void OnUpdate()
        {
            var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
            var anim = EntityManager.World.GetExistingSystem<PlayerAnimationSystem>();
            var tms = EntityManager.World.GetExistingSystem<TurnManagementSystem>();

            var time = Time.time;
            tms.CleanActionQueue();
            if (gss.IsInGame)
            {
                Entities.WithAll<PlayerInput>().ForEach((Entity player, ref WorldCoord coord) =>
                {
                    var action = GetAction(player, time);

                    if (action == Action.None)
                        return;


                    anim.StartAnimation(action);
                    tms.AddActionRequest(action, player, coord);
                    

                    // Save the action to the action stream if the player has it
                    if (!Replaying && EntityManager.HasComponent<ActionStream>(player))
                    {
                        var stream = EntityManager.GetBuffer<ActionStream>(player);
                        stream.Add(new ActionStream
                        {
                            action = action,
                            time = Time.time - StartTime
                        });
                    }
                });
            }
        }
    }
}
