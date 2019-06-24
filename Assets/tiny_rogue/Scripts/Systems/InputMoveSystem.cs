using System;
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
        protected override void OnUpdate() { }
        
        public void OnUpdateManual()
        {   
            Entities.WithAll<MoveWithInput>().ForEach((Entity player, ref WorldCoord coord, ref Translation translation) =>
            {
                var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
                var input = EntityManager.World.GetExistingSystem<InputSystem>();
                var turnManager = gss.TurnManager;

                var x = coord.x;
                var y = coord.y;

                var playerOnStairs = false;

                Entities.WithNone<BlockMovement>().WithAll<Stairway>().ForEach((ref WorldCoord stairCoord, ref Translation stairTrans) =>
                {
                    if (x == stairCoord.x && y == stairCoord.y)
                    {
                        playerOnStairs = true;
                    }
                });

                if (input.GetKeyDown(KeyCode.W) || input.GetKeyDown(KeyCode.UpArrow))
                    y = y - 1;
                if(input.GetKeyDown(KeyCode.S) || input.GetKeyDown(KeyCode.DownArrow))    
                    y = y + 1;
                if (input.GetKeyDown(KeyCode.D) || input.GetKeyDown(KeyCode.RightArrow))
                {
                    if(playerOnStairs)
                    {
                        gss.MoveToNextLevel();
                        return;
                    }
                    x = x + 1;
                }
                if(input.GetKeyDown(KeyCode.A) || input.GetKeyDown(KeyCode.LeftArrow))
                    x = x - 1;
                if (input.GetKeyDown(KeyCode.Space))
                {
                    var log = EntityManager.World.GetExistingSystem<LogSystem>();
                    log.AddLog("You wait a turn.");
                    turnManager.NeedToTickTurn = true;
                }

                bool wantsToMove = (x != coord.x || y != coord.y);

                if (!wantsToMove)
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
