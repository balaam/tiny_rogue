using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using Unity.Tiny.Input;

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
            var input = EntityManager.World.GetExistingSystem<InputSystem>();
            
            Entities.WithAll<MoveWithInput>().ForEach((ref WorldCoord coord, ref Translation translation) =>
            {
                if (input.GetKeyDown(KeyCode.W) || input.GetKeyDown(KeyCode.UpArrow))
                    translation.Value.y += TinyRogueConstants.TileHeight; //y = y - 1;// localPosition.y += 0.16;
                if(input.GetKeyDown(KeyCode.S) || input.GetKeyDown(KeyCode.DownArrow))    
                    translation.Value.y -= TinyRogueConstants.TileHeight;//y = y + 1;// localPosition.y -= 0.16;
                if(input.GetKeyDown(KeyCode.D) || input.GetKeyDown(KeyCode.RightArrow))
                    translation.Value.x += TinyRogueConstants.TileWidth; //x = x + 1;// localPosition.x += 0.09;
                if(input.GetKeyDown(KeyCode.A) || input.GetKeyDown(KeyCode.LeftArrow))
                    translation.Value.x -= TinyRogueConstants.TileWidth; // x = x - 1;// localPosition.x -= 0.09;      
            });
        }
    }
}
