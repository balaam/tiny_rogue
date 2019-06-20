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
        TurnManager _turnManager = new TurnManager();
        ArchetypeLibrary _archetypeLibrary = new ArchetypeLibrary();

        public View View => _view;
        public TurnManager TurnManager => _turnManager;
        public bool IsInGame => (_state == eGameState.InGame);

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

            _archetypeLibrary.Init(EntityManager);
            var startX = -(math.floor(width/2) * TinyRogueConstants.TileWidth);
            var startY = math.floor(height / 2) * TinyRogueConstants.TileHeight;

            _view.ViewTiles = new Entity[width * height];
            for (int i = 0; i < width * height; i++)
            {
                int2 xy = View.IndexToXY(i, width);
                float3 pos =  new float3(
                    startX + (xy.x * TinyRogueConstants.TileWidth), 
                    startY - (xy.y * TinyRogueConstants.TileHeight), 0);
                
                Entity instance = _archetypeLibrary.CreateTile(
                    EntityManager, xy, pos, mapEntity);
           
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
            
            // Hard code a couple of spear traps, so the player can die.
            var trap1Coord = new int2(12, 12);
            var trap2Coord = new int2(13, 11);
            _archetypeLibrary.CreateSpearTrap(EntityManager, trap1Coord, _view.ViewCoordToWorldPos(trap1Coord));
            _archetypeLibrary.CreateSpearTrap(EntityManager, trap2Coord, _view.ViewCoordToWorldPos(trap2Coord));

            var crownCoord = new int2(13, 12);
            _archetypeLibrary.CreateCrown(EntityManager, crownCoord, _view.ViewCoordToWorldPos(crownCoord));
        }

        protected override void OnUpdate()
        {
            switch (_state)
            {
                case eGameState.Startup:
                {
                    bool done = TryGenerateViewport();
                    if (done)
                       MoveToTitleScreen();
                    
                } break;
                case eGameState.Title:
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();
                    var log = EntityManager.World.GetExistingSystem<LogSystem>();
                    if (input.GetKeyDown(KeyCode.Space))
                    {
                        GenerateLevel();
                        TurnManager.ResetTurnCount();
                        log.AddLog("You are in a vast cavern.    Use the arrow keys to explore!");
                        log.AddLog("HAPPY HACKWEEK!");
                        
                        // Place the player
                        Entities.WithAll<MoveWithInput>().ForEach((Entity player, ref WorldCoord coord, ref Translation translation, ref HealthPoints hp) =>
                        {
                            coord.x = 10;
                            coord.y = 10;
                            translation.Value = View.ViewCoordToWorldPos(new int2(coord.x, coord.y));
                            
                            hp.max = TinyRogueConstants.StartPlayerHealth;
                            hp.now = hp.max;
                        });
                        _state = eGameState.InGame;
                    }
                } break;
                case eGameState.InGame:
                {
                    var input = World.GetExistingSystem<InputMoveSystem>();
                    var sbs = World.GetOrCreateSystem<StatusBarSystem>();
                    var ds = World.GetExistingSystem<DeathSystem>();
                    sbs.OnUpdateManual();  
                    input.OnUpdateManual();  
                    
                    if(TurnManager.NeedToTickTurn || TurnManager.TurnCount == 0)
                        TurnManager.OnTurn();
                    
                    ds.OnUpdateManual();
                    
                } break;
                case eGameState.GameOver:
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();
                    if (input.GetKeyDown(KeyCode.Space))
                        MoveToTitleScreen();                    
                } break;
            }
        }

        private void CleanUpGameWorld()
        {
            var log = EntityManager.World.GetExistingSystem<LogSystem>();
            log.Clear();
            // Clear the screen.
            Entities.WithAll<Tile>().ForEach((ref Sprite2DRenderer renderer) =>
            {
                renderer.sprite = SpriteSystem.AsciiToSprite[' '];
            });
            
            // Destroy everything that's not a tile or the player.
            Entities.WithNone<Tile, Player>().WithAll<WorldCoord>().ForEach((Entity entity, ref Translation t) =>
            {
                t.Value = TinyRogueConstants.OffViewport;
                PostUpdateCommands.DestroyEntity(entity);
            });
            
            Entities.WithAll<Player>().ForEach((ref Translation pos) =>
            {
                pos.Value = TinyRogueConstants.OffViewport;
            });  
        }

        public void MoveToTitleScreen()
        {
            // Clear the screen.
            Entities.WithAll<Tile>().ForEach((ref Sprite2DRenderer renderer) =>
            {
                renderer.sprite = SpriteSystem.AsciiToSprite[' '];
            });
            _view.Blit(EntityManager, new int2(0, 0), "TINY ROGUE");
            _view.Blit(EntityManager, new int2(30, 20),"PRESS SPACE TO BEGIN");
            _state = eGameState.Title;
        }

        public void MoveToGameOver()
        { 
            CleanUpGameWorld();
            _view.Blit(EntityManager, new int2(0, 0), "GAME OVER!");
            _view.Blit(EntityManager, new int2(30, 20),"PRESS SPACE TO TRY AGAIN");
            _state = eGameState.GameOver;
        }
        
        public void MoveToGameWin()
        {
            CleanUpGameWorld(); 
            _view.Blit(EntityManager, new int2(0, 0), "YOU WIN!");
            _view.Blit(EntityManager, new int2(30, 20),"PRESS SPACE TO START AGAIN");
            _state = eGameState.GameOver;
        }
    }
}