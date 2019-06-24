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
        
        public void OnUpdateManual(EntityCommandBuffer commandBuffer)
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
                    var log = EntityManager.World.GetExistingSystem<LogSystem>();
                    log.AddLog("You wait a turn.");
                    turnManager.NeedToTickTurn = true;
                }

                bool wantsToMove = (x != coord.x || y != coord.y);

                if (!wantsToMove)
                    return;
                
                Entities.WithNone<BlockMovement>().WithAll<Tile>().ForEach((ref WorldCoord tileCoord, ref Translation tileTrans) =>
                {
                    if (!input.GetKey(KeyCode.LeftControl))
                    {
                        // This location the player wants to move has nothing blocking them, so update their position.
                        if (tileCoord.x == x && tileCoord.y == y)
                        {
                            EntityManager.SetComponentData(player, tileCoord);
                            EntityManager.SetComponentData(player, tileTrans);
                            turnManager.NeedToTickTurn = true;
                        }
                    }
                });

                Entities.WithAll<Door>().ForEach((Entity doorEntity, ref WorldCoord tileCoord, ref Sprite2DRenderer renderer, ref Door door) =>
                {
                    if (tileCoord.x == x && tileCoord.y == y)
                    {
                        var log = EntityManager.World.GetExistingSystem<LogSystem>();
                        if (!door.Opened)
                        {
                            log.AddLog("You opened a door.");
                            door.Opened = true;
                            commandBuffer.RemoveComponent(doorEntity, typeof(BlockMovement));
                            renderer.sprite = SpriteSystem.AsciiToSprite['\\'];
                        }
                        else if(input.GetKey(KeyCode.LeftControl))
                        {
                            log.AddLog("You closed a door.");
                            door.Opened = false;
                            commandBuffer.AddComponent(doorEntity, new BlockMovement());
                            renderer.sprite = SpriteSystem.AsciiToSprite['|'];
                        }

                        turnManager.NeedToTickTurn = true;
                    }
                });
            });
        }
    }
}
