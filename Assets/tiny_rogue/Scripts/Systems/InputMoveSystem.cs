using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
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
        protected override void OnUpdate()
        {
            Entities.WithAll<MoveWithInput>().ForEach((Entity player, ref WorldCoord coord, ref Translation translation) =>
            {
                var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
                var input = EntityManager.World.GetExistingSystem<InputSystem>();
                var turnManager = gss.TurnManager;

                var x = coord.x;
                var y = coord.y;
                
                if (input.GetKeyDown(KeyCode.W) || input.GetKeyDown(KeyCode.UpArrow))
                    y = y - 1;
                if(input.GetKeyDown(KeyCode.S) || input.GetKeyDown(KeyCode.DownArrow))    
                    y = y + 1;
                if(input.GetKeyDown(KeyCode.D) || input.GetKeyDown(KeyCode.RightArrow))
                    x = x + 1;
                if(input.GetKeyDown(KeyCode.A) || input.GetKeyDown(KeyCode.LeftArrow))
                    x = x - 1;
                if (input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("Wait");
                    turnManager.NeedToTickTurn = true;
                }

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
