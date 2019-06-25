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
    struct TimedAction
    {
        public Action action;
        public float time;
    }

    [UpdateAfter(typeof(StatusBarSystem))]
    public class PlayerInputSystem : ComponentSystem
    {
        private bool Replaying = false;
        private float StartTime;

        private bool alternateAction = false;

        private List<TimedAction> ActionStream = new List<TimedAction>();

        public void StartRecording()
        {
            Replaying = false;
            StartTime = Time.time;
            ActionStream.Clear();
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
            if (ActionStream.Count > 0)
                return Action.None;
            
            var action = ActionStream[0];

            // Don't run if we've not reached the right time yet
            if (time < action.time)
                return Action.None;

            // Remove and run the action
            ActionStream.RemoveAt(0);
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

            var time = Time.time;

            if (gss.IsInGame)
            {
                Entities.WithAll<PlayerInput>().ForEach((Entity playerEntity, ref Player player, ref WorldCoord coord) =>
                {
                    // In Graphical, you have to wait for the animation of the action to complete first.
                    if (!GlobalGraphicsSettings.ascii)
                    {
                        var currentAction = EntityManager.GetComponentData<Player>(playerEntity).Action;
                        if (currentAction != Action.None) return;
                    }

                    var action = GetAction(playerEntity, time);

                    if (action == Action.None)
                        return;
                        

                    var pas = EntityManager.World.GetExistingSystem<PlayerActionSystem>();
                    var anim = EntityManager.World.GetExistingSystem<PlayerAnimationSystem>();

                    bool moved = false;
                    switch (action)
                    {
                        case Action.MoveUp:
                        case Action.MoveDown:
                        case Action.MoveRight:
                        case Action.MoveLeft:
                            var move = GetMove(action);
                            moved = pas.TryMove(playerEntity, new WorldCoord { x = coord.x + move.x, y = coord.y + move.y }, alternateAction, PostUpdateCommands);
                            break;
                        case Action.Interact:
                            pas.Interact(coord, PostUpdateCommands);
                            break;
                        case Action.Wait:
                            Debug.Log("Wait is happening.");
                            pas.Wait();
                            break;
                        case Action.None:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("Unhandled input");
                    }

                    if (!GlobalGraphicsSettings.ascii)
                    {
                        Debug.Log($"Animate {(int)action} {moved}");
                        anim.StartAnimation(action, moved);
                    }

                    // Save the action to the action stream if the player has it
                    if (!Replaying)
                    {
                        ActionStream.Add(new TimedAction()
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
