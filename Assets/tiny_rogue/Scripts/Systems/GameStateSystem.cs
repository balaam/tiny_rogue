using System;
using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Mathematics;
using UnityEngine;
using Unity.Tiny.Input;
using KeyCode = Unity.Tiny.Input.KeyCode;
#if !UNITY_WEBGL
using InputSystem = Unity.Tiny.GLFW.GLFWInputSystem;
#else
    using InputSystem =  Unity.Tiny.HTML.HTMLInputSystem;
#endif

namespace game
{
    // GameState drives the other systems.
    public class GameStateSystem : ComponentSystem
    {
        // Simple game loop
        // StartUp -> Title -> InGame -> GameOver -> Title -> loop 
        public enum eGameState
        {
            Startup, // generate required entities etc
            Title,
            InGame,
            GameOver,
        }

        eGameState _state = eGameState.Startup;
        View _view = new View();

        private bool TryGenerateViewport()
        {
            if (!SpriteSystem.Loaded) // can't make the viewport without sprites.
                return false;
            
            Entity mapEntity = Entity.Null;

            int width = -1;
            int height = -1;
            
            bool foundMap = false;
            
            Entities.ForEach((Entity entity, ref Viewport view) =>
            {
                mapEntity = entity;
                width = view.width;
                height = view.height;
                foundMap = true;
            });
            
            if (!foundMap)
                return false;
            
            var a = EntityManager.CreateArchetype(new ComponentType[]
            {
                typeof(Parent),
                typeof(Translation),
                typeof(WorldCoord), // should be view coord?
                typeof(Rotation),
                typeof(Sprite2DRenderer),
                typeof(LayerSorting),
                typeof(Tile)
            });
            
            var startX = -(math.floor(width/2) * TinyRogueConstants.TileWidth);
            var startY = math.floor(height / 2) * TinyRogueConstants.TileHeight;

            _view.ViewTiles = new Entity[width * height];
            for (int i = 0; i < width * height; i++)
            {
                int2 xy = View.IndexToXY(i, width);
                
                Entity e = EntityManager.CreateEntity(a);
                Sprite2DRenderer s = new Sprite2DRenderer();
                Translation t = new Translation();
                Parent p = new Parent();
                WorldCoord c = new WorldCoord(); // ViewCoord?
                p.Value = mapEntity;
                t.Value = new float3(
                    startX + (xy.x * TinyRogueConstants.TileWidth), 
                    startY - (xy.y * TinyRogueConstants.TileHeight), 0);
                
                c.x = xy.x;
                c.y = xy.y;
                
                s.color = new Unity.Tiny.Core2D.Color(1, 1, 1, 1);
                s.sprite = SpriteSystem.AsciiToSprite[' '];
                
                EntityManager.SetComponentData(e, s);
                EntityManager.SetComponentData(e, t);
                EntityManager.SetComponentData(e, p);
                EntityManager.SetComponentData(e, c);

                Entity instance = EntityManager.Instantiate(e);
                this._view.ViewTiles[i] = instance;
            }

            this._view.Width = width;
            this._view.Height = height;
            return true;
        }

        
        public void GenerateLevel()
        {
            // Removing blocking tags from all tiles
            Entities.WithAll<BlockMovement, Tile>().ForEach((Entity entity) =>
            {
                EntityManager.RemoveComponent(entity, typeof(BlockMovement)); 
            });

            Entities.WithAll<Tile>().ForEach((Entity e, ref WorldCoord tileCoord, ref Sprite2DRenderer renderer) =>
            {
                var x = tileCoord.x;
                var y = tileCoord.y;
                
                bool isVWall = (x == 0 || x == _view.Width - 1) && y > 0 && y < _view.Height - 2;
                bool isHWall = (y == 1 || y == _view.Height - 2);
                
                if(isVWall || isHWall)
                {
                    renderer.sprite = SpriteSystem.AsciiToSprite['#'];
                    PostUpdateCommands.AddComponent<BlockMovement>(e, new BlockMovement());
                }
                else
                {
                    renderer.sprite = SpriteSystem.AsciiToSprite['.'];
                }
            });
        } 
       

        protected override void OnUpdate()
        {
            // In Startup create the viewport
            if (_state == eGameState.Startup)
            {
                bool done = TryGenerateViewport();
                if (done)
                {
                    Debug.Log("Moving to Title Screen State.");
                    _view.Blit(EntityManager, 0, 0, "TINY ROGUE");
                    _view.Blit(EntityManager, 30, 20,"PRESS SPACE TO BEGIN");
                    _state = eGameState.Title;
                }
            }
            else if(_state == eGameState.Title)
            {
                // Wait for space
                var input = EntityManager.World.GetExistingSystem<InputSystem>();
                if (input.GetKeyDown(KeyCode.Space))
                {
                    GenerateLevel();
                    _state = eGameState.InGame;
                }
            }
        }
    }
}