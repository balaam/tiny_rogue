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
        private Action GetAction()
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
        
        protected override void OnUpdate() 
        { 
            var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
            if (gss.IsInGame)
            {
                var action = GetAction();

                if (action == Action.None)
                    return;
                
                Entities.WithAll<PlayerInput>().ForEach((Entity player, ref WorldCoord coord) =>
                {
                    var pas = EntityManager.World.GetExistingSystem<PlayerActionSystem>();
                    
                    switch (action)
                    {
                        case Action.MoveUp:
                        case Action.MoveDown:
                        case Action.MoveRight:
                        case Action.MoveLeft:
                            var move = GetMove(action);
                            pas.TryMove(player, new WorldCoord { x = coord.x + move.x, y = coord.y + move.y });
                            break;
                        case Action.Interact:
                            pas.Interact(coord);
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
                    
                    // Save the action to the action stream if the player has it
                    if (EntityManager.HasComponent<ActionStream>(player))
                    {
                        var stream = EntityManager.GetBuffer<ActionStream>(player);
                        stream.Add(new ActionStream {action = action, time = Time.time});
                    }
                });
            }
        }
    }
}
