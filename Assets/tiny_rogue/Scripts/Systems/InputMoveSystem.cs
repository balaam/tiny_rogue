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
        protected override void OnUpdate()
        {
            //var x = World.GetOrCreateSystem<InventorySystem>();
            //x.AddItem();

        }
        
        public void OnUpdateManual() 
        {
            Entities.WithAll<MoveWithInput>().ForEach(
                (Entity player, ref WorldCoord coord, ref Translation translation) =>
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();

                    if (input.GetKeyDown(KeyCode.W) || input.GetKeyDown(KeyCode.UpArrow))
                        translation.Value.y = translation.Value.y + TinyRogueConstants.TileHeight;
                    if (input.GetKeyDown(KeyCode.S) || input.GetKeyDown(KeyCode.DownArrow))
                        translation.Value.y = translation.Value.y - TinyRogueConstants.TileHeight;
                    if (input.GetKeyDown(KeyCode.D) || input.GetKeyDown(KeyCode.RightArrow))
                        translation.Value.x = translation.Value.x + TinyRogueConstants.TileWidth;
                    if (input.GetKeyDown(KeyCode.A) || input.GetKeyDown(KeyCode.LeftArrow))
                        translation.Value.x = translation.Value.x -  TinyRogueConstants.TileWidth;
                    if (input.GetKeyDown(KeyCode.Space))
                    {
                        // Wait not implemented.
                    }
                });
        }
    }
}
