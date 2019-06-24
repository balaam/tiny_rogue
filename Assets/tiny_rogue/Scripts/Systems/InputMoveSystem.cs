using System;
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
    public class InputMoveSystem : ComponentSystem
    {
        enum Action
        {
            MoveUp,
            MoveDown,
            MoveLeft,
            MoveRight,
            Wait,
            None
        }
        
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
            if (input.GetKeyDown(KeyCode.Space))
                return Action.Wait;

            return Action.None;
        }
        
        public void OnUpdateManual()
        {   
            Entities.WithAll<MoveWithInput>().ForEach((Entity player, ref WorldCoord coord, ref Translation translation) =>
            {
                var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
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
                    case Action.Wait:
                    {
                        var log = EntityManager.World.GetExistingSystem<LogSystem>();
                        log.AddLog("You wait a turn.");
                        turnManager.NeedToTickTurn = true;
                    } break;
                    case Action.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Unhandled input");
                }
                
                if (x == coord.x && y == coord.y)
                    return;
                
                Entities.WithNone<BlockMovement>().WithAll<Tile>().ForEach((ref WorldCoord tileCoord, ref Translation tileTrans) =>
                {
                    // This location the player wants to move has nothing blocking them, so update their position.
                    if (tileCoord.x == x && tileCoord.y == y)
                    {
                        EntityManager.SetComponentData(player, tileCoord);
                        EntityManager.SetComponentData(player, tileTrans);                        
                        turnManager.NeedToTickTurn = true;
                    }
                });
            });
        }
    }
}
