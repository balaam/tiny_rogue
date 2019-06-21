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
        GameMap _map = new GameMap();
        //TurnManager _turnManager = new TurnManager();
        ArchetypeLibrary _archetypeLibrary = new ArchetypeLibrary();

       // public GameMap Map => _map;
        //public TurnManager TurnManager => _turnManager;
        public bool IsInGame => (_state == eGameState.InGame);

        private bool TryGenerateMap()
        {
            if (!SpriteSystem.Loaded) // can't make the map without sprites.
                return false;
            
            Entity mapEntity = Entity.Null;

            int width = -1;
            int height = -1;
            
            bool foundMap = false;
            
            Entities.ForEach((Entity entity, ref Map map) =>
            {
                mapEntity = entity;
                width = map.width;
                height = map.height;
                foundMap = true;
            });
            
            if (!foundMap)
                return false;

            _archetypeLibrary.Init(EntityManager);
            var startX = -(math.floor(width/2) * TinyRogueConstants.TileWidth);
            var startY = math.floor(height / 2) * TinyRogueConstants.TileHeight;

            _map.ViewTiles = new Entity[width * height];
            for (int i = 0; i < width * height; i++)
            {
                int2 xy = GameMap.IndexToXY(i, width);
                float3 pos =  new float3(
                    startX + (xy.x * TinyRogueConstants.TileWidth), 
                    startY - (xy.y * TinyRogueConstants.TileHeight), 0);
                
                Entity instance = _archetypeLibrary.CreateTile(
                    EntityManager, xy, pos, mapEntity);
           
                _map.ViewTiles[i] = instance;
            }

            _map.Width = width;
            _map.Height = height;
            return true;
        }



        protected override void OnUpdate()
        {
            switch (_state)
            {
                case eGameState.Startup:
                {
                    bool done = TryGenerateMap();
                    if (done)
                    {
                        _state = eGameState.InGame;
                    }

                } break;
                case eGameState.InGame:
                {
                    var input = World.GetExistingSystem<InputMoveSystem>();  
                    input.OnUpdateManual();  
                } break;
            }
        }
    }
}