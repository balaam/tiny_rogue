using System;
using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Mathematics;
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
            ReadQueuedLog,
            Replay,
            GameOver,
            NextLevel,
            DebugLevelSelect,
        }

        eGameState _state = eGameState.Startup;
        View _view = new View();
        ArchetypeLibrary _archetypeLibrary = new ArchetypeLibrary();
        DungeonSystem _dungeon;

        public View View => _view;
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
            var startX = -(math.floor(width / 2) * GlobalGraphicsSettings.TileSize.x);
            var startY = math.floor(height / 2) * GlobalGraphicsSettings.TileSize.y;

            _view.ViewTiles = new Entity[width * height];
            for (int i = 0; i < width * height; i++)
            {
                int2 xy = View.IndexToXY(i, width);
                float3 pos =  new float3(
                    startX + (xy.x * GlobalGraphicsSettings.TileSize.x),
                    startY - (xy.y * GlobalGraphicsSettings.TileSize.y), 0);

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
            _dungeon.GenerateDungeon(_view);

            // Hard code a couple of spear traps, so the player can die.
            var trap1Coord = _dungeon.GetRandomPositionInRandomRoom();
            var trap2Coord = _dungeon.GetRandomPositionInRandomRoom();
            _archetypeLibrary.CreateSpearTrap(EntityManager, trap1Coord, _view.ViewCoordToWorldPos(trap1Coord));
            _archetypeLibrary.CreateSpearTrap(EntityManager, trap2Coord, _view.ViewCoordToWorldPos(trap2Coord));

            var stairwayCoord = _dungeon.GetRandomPositionInRandomRoom();
            _archetypeLibrary.CreateStairway(EntityManager, stairwayCoord, _view.ViewCoordToWorldPos(stairwayCoord));

            var crownCoord = _dungeon.GetRandomPositionInRandomRoom();
            _archetypeLibrary.CreateCrown(EntityManager, crownCoord, _view.ViewCoordToWorldPos(crownCoord));

            //TODO: random positions for gold
            var goldCoord = _dungeon.GetRandomPositionInRandomRoom();
            _archetypeLibrary.CreateGold(EntityManager, goldCoord, _view.ViewCoordToWorldPos(goldCoord));
           
            var collectibleCoord = new int2(15,12);
            _archetypeLibrary.CreateCollectible(EntityManager, collectibleCoord, _view.ViewCoordToWorldPos(collectibleCoord));
  
       }

        public void GenerateCombatTestLevel()
        {
            _dungeon.GenerateDungeon(_view);

            int2 dummyCoord = _dungeon.GetRandomPositionInRandomRoom();
            _archetypeLibrary.CreateCombatDummy(EntityManager, dummyCoord, _view.ViewCoordToWorldPos(dummyCoord));
            
            // Create 'Exit'
            var crownCoord = _dungeon.GetRandomPositionInRandomRoom();
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
                        {
                            _dungeon = EntityManager.World.GetExistingSystem<DungeonSystem>();
                            MoveToTitleScreen();
                        }

                    }
                    break;
                case eGameState.Title:
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();
                    var log = EntityManager.World.GetExistingSystem<LogSystem>();
                    if (input.GetKeyDown(KeyCode.D))
                    {
                        MoveToDebugLevelSelect();
                    }
                    else if (input.GetKeyDown(KeyCode.R))
                    {
                        
                    }
                    else if (input.GetKeyUp(KeyCode.Space))
                    {
                        var tms = EntityManager.World.GetExistingSystem<TurnManagementSystem>();
                        GenerateLevel();
                        tms.ResetTurnCount();
                        log.AddLog("You are in a vast cavern.    Press Space for next log");
                        log.AddLog("HAPPY HACKWEEK!    Use the arrow keys to explore!");
                        _state = eGameState.InGame;
                    }
                } break;
                case eGameState.InGame:
                {

                } break;
                case eGameState.ReadQueuedLog:
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();
                    var log = EntityManager.World.GetExistingSystem<LogSystem>();

                    if (log.HasQueuedLogs())
                    {
                        if (input.GetKeyDown(KeyCode.Space))
                            log.ShowNextLog();
                    }
                    else
                    {
                        _state = eGameState.InGame;
                    }
                } break;
                case eGameState.Replay:
                {
                    // TODO: Replay recorded input
                } break;
                case eGameState.GameOver:
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();
                    if (input.GetKeyDown(KeyCode.Space))
                        MoveToTitleScreen();
                } break;
                case eGameState.NextLevel:
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();
                    var log = EntityManager.World.GetExistingSystem<LogSystem>();
                    if (input.GetKeyDown(KeyCode.Space))
                    {
                        GenerateLevel();
                        log.AddLog("You are in a vast cavern.    Use the arrow keys to explore!");
                        _state = eGameState.InGame;
                    }
                    } break;
                case eGameState.DebugLevelSelect:
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();
                    var log = EntityManager.World.GetExistingSystem<LogSystem>();

                    if (input.GetKeyDown(KeyCode.Alpha1))
                    {
                        var tms = EntityManager.World.GetExistingSystem<TurnManagementSystem>();
                        GenerateCombatTestLevel();
                        tms.ResetTurnCount();
                        log.AddLog("This is a room to test the combat system");
                        log.AddLog("Move to the crown to exit");
                        _state = eGameState.InGame;
                    }
                    else if (input.GetKeyDown(KeyCode.Space))
                    {
                        MoveToTitleScreen();
                    }
                } break;
            }
        }

        private void CleanUpGameWorld(EntityCommandBuffer cb)
        {
            var log = EntityManager.World.GetExistingSystem<LogSystem>();
            log.Clear();
            // Clear the screen.
            Entities.WithAll<Tile>().ForEach((ref Sprite2DRenderer renderer) =>
            {
                renderer.sprite = SpriteSystem.IndexSprites[GlobalGraphicsSettings.ascii ? ' ' : 0];
            });

            // Destroy everything that's not a tile or the player.
            Entities.WithNone<Tile, Player>().WithAll<WorldCoord>().ForEach((Entity entity, ref Translation t) =>
            {
                t.Value = TinyRogueConstants.OffViewport;
                cb.DestroyEntity(entity);
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
                renderer.sprite = SpriteSystem.IndexSprites[GlobalGraphicsSettings.ascii ? ' ' : 0 ];
            });
            _view.Blit(EntityManager, new int2(0, 0), "TINY ROGUE");
            _view.Blit(EntityManager, new int2(30, 20),"PRESS SPACE TO BEGIN");
            _view.Blit(EntityManager, new int2(70, 23),"(d)ebug");
            _state = eGameState.Title;
        }

        public void MoveToGameOver(EntityCommandBuffer cb)
        { 
            CleanUpGameWorld(cb);
            _view.Blit(EntityManager, new int2(0, 0), "GAME OVER!");
            _view.Blit(EntityManager, new int2(30, 20),"PRESS SPACE TO TRY AGAIN");
            _state = eGameState.GameOver;
        }
        
        public void MoveToGameWin(EntityCommandBuffer cb)
        {
            CleanUpGameWorld(cb);
            _view.Blit(EntityManager, new int2(0, 0), "YOU WIN!");
            _view.Blit(EntityManager, new int2(30, 20),"PRESS SPACE TO START AGAIN");
            _state = eGameState.GameOver;
        }

        public void MoveToNextLevel(EntityCommandBuffer cb)
        {
            CleanUpGameWorld(cb);
            _view.Blit(EntityManager, new int2(0, 0), "YOU FOUND STAIRS LEADING DOWN");
            _view.Blit(EntityManager, new int2(30, 20), "PRESS SPACE TO CONTINUE");
            _state = eGameState.NextLevel;
        }

        private void MoveToDebugLevelSelect()
        {
            // Clear the screen.
            Entities.WithAll<Tile>().ForEach((ref Sprite2DRenderer renderer) =>
            {
                // TODO: need to figure out empty/none tile
                renderer.sprite = SpriteSystem.IndexSprites[GlobalGraphicsSettings.ascii ? ' ' : 0 ];
            });
            _view.Blit(EntityManager, new int2(0, 0), "TINY ROGUE (Debug Levels)");
            _view.Blit(EntityManager, new int2(30, 10),"1) Combat Test");
            _view.Blit(EntityManager, new int2(30, 20),"PRESS SPACE TO EXIT");
            _state = eGameState.DebugLevelSelect;
        }

        public void MoveToReadQueuedLog()
        {
            _state = eGameState.ReadQueuedLog;
        }
    }
}
