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
    public enum Action
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Wait,
        Interact,
        None
    }
    
    public class PlayerInputSystem : ComponentSystem
    {
        protected override void OnUpdate() { }

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
        
        public void OnUpdateManual()
        {   
            Entities.WithAll<PlayerInput>().ForEach((Entity player, ref WorldCoord coord, ref Translation translation) =>
            {
                var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
                var rec = EntityManager.World.GetExistingSystem<PlayerInputRecordSystem>();
                var turnManager = gss.TurnManager;

                var x = coord.x;
                var y = coord.y;

                var action = GetAction();
                switch (action)
                {
                    case Action.MoveUp:
                        y = y - 1;
                        break;
                    case Action.MoveDown:
                        y = y + 1;
                        break;
                    case Action.MoveRight:
                        x = x + 1;
                        break;
                    case Action.MoveLeft:
                        x = x - 1;
                        break;
                    case Action.Interact:
                        gss.Interact(x,y);
                        break;
                    case Action.Wait:
                        gss.Wait();
                        break;
                    case Action.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Unhandled input");
                }
                
                // Save the action to the action stream
                if( action != Action.None )
                    rec.AddAction(action);
                
                // Move if we've moved
                if (x != coord.x || y != coord.y)
                    gss.TryMove(player, x, y);

            });
        }
    }
}
