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
        private int ReplayPosition = 0;
        
        private static float ReplaySpeed = 0.125f;
        private static float LastTime = 0.0f;

        private List<Action> ActionStream = new List<Action>();

        public void StartRecording()
        {
            Replaying = false;
            ActionStream.Clear();
        }

        public void StartReplaying()
        {
            Replaying = true;
            ReplayPosition = 0;
        }

        private Action GetActionFromInput()

        {
            var input = EntityManager.World.GetExistingSystem<InputSystem>();

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

        private Action GetActionFromActionStream(float time)
        {
            // Bail when we're outside of the stream
            if (ReplayPosition >= ActionStream.Count)
                return Action.None;

            var action = ActionStream[ReplayPosition];

            // Don't run if we've not reached the right time yet
            if (time < LastTime + ReplaySpeed)
                return Action.None;
            
            ReplayPosition++;
            LastTime = time;
            return action;

        }

        protected override void OnUpdate()
        {
            var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
            var anim = EntityManager.World.GetExistingSystem<AnimationSystem>();
            var tms = EntityManager.World.GetExistingSystem<TurnManagementSystem>();

            var time = Time.time;
            tms.CleanActionQueue();

            if (gss.IsInGame)
            {
                Entities.WithAll<PlayerInput>().ForEach((Entity playerEntity, ref Animated animated, ref WorldCoord coord) =>
                {
                    // In Graphical, you have to wait for the animation of the action to complete first.
                    if (!GlobalGraphicsSettings.ascii)
                    {
                        var currentAction = animated.Action;
                        if (currentAction != Action.None) return;
                    }

                    var action = Replaying ? GetActionFromActionStream(time) : GetActionFromInput();

                    if (action == Action.None)
                        return;

                    tms.AddActionRequest(action, playerEntity, coord);
                    
                    bool moved = false;
                    if (!GlobalGraphicsSettings.ascii)
                    {
                        Debug.Log($"Animate {(int)action} {moved}");
                        anim.StartAnimation(playerEntity, action, moved);
                    }

                    // Save the action to the action stream if the player has it
                    if (!Replaying)
                        ActionStream.Add(action);
                });
            }
        }
    }
}
