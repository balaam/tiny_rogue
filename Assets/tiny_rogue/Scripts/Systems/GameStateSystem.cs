using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Input;
using UnityEngine;
using Color = Unity.Tiny.Core2D.Color;
using KeyCode = Unity.Tiny.Input.KeyCode;
using Random = Unity.Mathematics.Random;
#if !UNITY_WEBGL
using InputSystem = Unity.Tiny.GLFW.GLFWInputSystem;
#else
    using InputSystem =  Unity.Tiny.HTML.HTMLInputSystem;
#endif

namespace game
{
    // GameState drives the other systems.
    [UpdateBefore(typeof(TurnManagementSystem))]
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
            GameOver,
            NextLevel,
            HiScores,
            Inventory,
        };

        public static View GameView = new View();

        private eGameState _state = eGameState.Startup;
        private ScoreManager _scoreManager = new ScoreManager();
        private ArchetypeLibrary _archetypeLibrary = new ArchetypeLibrary();
        private CreatureLibrary _creatureLibrary = new CreatureLibrary();

        private DungeonSystem _dungeon;

        private uint CurrentSeed = 1;
        public int CurrentLevel = 0;
        private int LastDungeonNumber;
        
        List<Sprite2DRenderer> spriteRenderers = new List<Sprite2DRenderer>();
        bool populateInventory = false;


        private uint MakeNewRandom()
        {
            return (uint)(Time.time * 100000.0);
        }

        public View View => GameView;
        public bool IsInGame => (_state == eGameState.InGame);

        protected override void OnCreate()
        {
            base.OnCreate();
            _creatureLibrary.Init(EntityManager);
        }

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

            GameView.ViewTiles = new Entity[width * height];
            for (int i = 0; i < width * height; i++)
            {
                int2 xy = View.IndexToXY(i, width);
                float3 pos =  new float3(
                    startX + (xy.x * GlobalGraphicsSettings.TileSize.x),
                    startY - (xy.y * GlobalGraphicsSettings.TileSize.y), 0);

                Entity instance = _archetypeLibrary.CreateTile(
                    EntityManager, xy, pos, mapEntity);

                GameView.ViewTiles[i] = instance;
            }

            GameView.Width = width;
            GameView.Height = height;
            return true;
        }

        public void GenerateLevel()
        {
            CurrentLevel++;
            CleanUpGameWorld(PostUpdateCommands);
            bool finalLevel = CurrentLevel == LastDungeonNumber;
            _dungeon.GenerateDungeon(PostUpdateCommands, GameView, _creatureLibrary, _archetypeLibrary, CurrentLevel, finalLevel);

        }

        void ClearView(EntityCommandBuffer ecb)
        {
            Entities.WithAll<Tile>().ForEach((Entity e, ref Sprite2DRenderer renderer) =>
            {
                ecb.SetComponent(e, new Sprite2DRenderer
                {
                    sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics(' ')]
                });
            });
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
                        var playerEntity = _creatureLibrary.SpawnPlayer(EntityManager);
                        // Re-parent the camera on graphical to follow the character.
                        if (!GlobalGraphicsSettings.ascii)
                        {
                            Entities.WithAll<Camera2D>().ForEach((ref Parent parent) =>
                            {
                                parent.Value = playerEntity;
                            });
                        }

                        _dungeon = EntityManager.World.GetExistingSystem<DungeonSystem>();
                        MoveToTitleScreen(PostUpdateCommands);
                    }
                }
                break;
                case eGameState.Title:
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();
                    if(input.GetKeyDown(KeyCode.H))
                    {
                        MoveToHiScores(PostUpdateCommands);
                    }
                    else if (input.GetKeyDown(KeyCode.Space))
                    {
                        MoveToInGame(PostUpdateCommands);
                    }
                } break;
                case eGameState.InGame:
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();
                    if (input.GetKeyDown(KeyCode.I))
                    {
                        MoveToInventoryScreen(PostUpdateCommands);
                    }

                } break;
                case eGameState.Inventory:
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();
                    if (populateInventory)
                    {
                        PopulateInventory();
                    }
                    if (input.GetKeyDown(KeyCode.Escape))
                    {
                        MoveBackToGame(PostUpdateCommands);
                    }

                } break;
                case eGameState.ReadQueuedLog:
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();
                    var log = EntityManager.World.GetExistingSystem<LogSystem>();

                    if (log.HasQueuedLogs())
                    {
                        if (input.GetKeyDown(KeyCode.Space))
                            log.ShowNextLog(PostUpdateCommands);
                    }
                    else
                    {
                        _state = eGameState.InGame;
                    }
                } break;
                case eGameState.GameOver:
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();
                    if (input.GetKeyDown(KeyCode.Space))
                        MoveToTitleScreen(PostUpdateCommands);
                    else if (input.GetKeyDown(KeyCode.R))
                            MoveToReplay(PostUpdateCommands);
                } break;
                case eGameState.NextLevel:
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();
                    var log = EntityManager.World.GetExistingSystem<LogSystem>();

                    GenerateLevel();
                    log.AddLog("You descend another floor.");
                    log.ShowNextLog(PostUpdateCommands);
                    _state = eGameState.InGame;
                } break;
                case eGameState.HiScores:
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();
                    if (input.GetKeyDown(KeyCode.Space))
                        MoveToTitleScreen(PostUpdateCommands);
                } break;
            }
        }


        private void CleanUpGameWorld(EntityCommandBuffer cb)
        {
            var log = EntityManager.World.GetExistingSystem<LogSystem>();
            log.Clear();

            // Clear the screen
            ClearView(cb);

            // Clear the dungeon
            _dungeon.ClearDungeon(cb, GameView);

            // Clear all Tile data
            Entities.WithAll<Tile>().ForEach((Entity e) =>
            {
                cb.SetComponent(e, new Tile());
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

        public void MoveToTitleScreen(EntityCommandBuffer cb)
        {
            HideInventory(cb);

            // Record every player move at title screen
            var pis = EntityManager.World.GetExistingSystem<PlayerInputSystem>();
            pis.StartRecording();

            // Start with a nice new seed
            CurrentSeed = MakeNewRandom();
            RandomRogue.Init(CurrentSeed);

            // Clear the screen.
            Entities.WithAll<Player>().ForEach((Entity player, ref GoldCount gc, ref ExperiencePoints xp) =>
            {
                _scoreManager.SetHiScores(gc.count + xp.now + (CurrentLevel - 1) * 10);
            });
            ClearView(cb);

            GameView.Blit(cb, new int2(0, 0), "TINY ROGUE");
            GameView.Blit(cb, new int2(30, 20),"PRESS SPACE TO BEGIN");
            GameView.Blit(cb, new int2(30, 21), "PRESS H FOR HISCORES");
            _state = eGameState.Title;
        }

        public void MoveToInGame( EntityCommandBuffer cb )
        {
            // Start at 0th level
            CurrentLevel = 0;
            LastDungeonNumber = RandomRogue.Next(5, 10);

            var log = EntityManager.World.GetExistingSystem<LogSystem>();
            var tms = EntityManager.World.GetExistingSystem<TurnManagementSystem>();

            GenerateLevel();
            tms.ResetTurnCount();
            log.AddLog("Welcome! Use the arrow keys to explore, z to interact and x to wait.");
            _state = eGameState.InGame;
        }

        void MoveToInventoryScreen(EntityCommandBuffer cb)
        {
            ShowInventory(cb);
            _state = eGameState.Inventory;
        }

        void MoveBackToGame(EntityCommandBuffer cb)
        {
            HideInventory(cb);
            _state = eGameState.InGame;
        }

        void MoveToReplay(EntityCommandBuffer cb)
        {
            var pis = EntityManager.World.GetExistingSystem<PlayerInputSystem>();

            pis.StartReplaying();

            // Init at the start of the current seed again
            RandomRogue.Init(CurrentSeed);

            MoveToInGame(cb);
        }



        public void MoveToGameOver(EntityCommandBuffer cb)
        {
            CleanUpGameWorld(cb);
            GameView.Blit(cb, new int2(0, 0), "GAME OVER!");
            GameView.Blit(cb, new int2(30, 20),"PRESS SPACE TO TRY AGAIN");
            GameView.Blit(cb, new int2(30, 21),"PRESS R FOR REPLAY");
            _state = eGameState.GameOver;
        }

        public void MoveToGameWin(EntityCommandBuffer cb)
        {
            CleanUpGameWorld(cb);
            GameView.Blit(cb, new int2(0, 0), "YOU WIN!");
            GameView.Blit(cb, new int2(30, 20),"PRESS SPACE TO START AGAIN");
            GameView.Blit(cb, new int2(30, 21),"PRESS R FOR REPLAY");
            _state = eGameState.GameOver;
        }

        public void MoveToNextLevel(EntityCommandBuffer cb)
        {
            CleanUpGameWorld(cb);
            GameView.Blit(cb, new int2(0, 0), "YOU FOUND STAIRS LEADING DOWN");
            GameView.Blit(cb, new int2(30, 20), "PRESS SPACE TO CONTINUE");
            _state = eGameState.NextLevel;
        }

        public void MoveToHiScores(EntityCommandBuffer cb)
        {
            CleanUpGameWorld(cb);
            GameView.Blit(cb, new int2(30, 7), "HiScores");
            GameView.Blit(cb, new int2(25, 20), "Press Space to Continue");
            for (int i = 1; i < 11; i++)
            {
                GameView.Blit(cb, new int2(30, 7 + (1 * i)), i.ToString() + ": ");
                GameView.Blit(cb, new int2(35, 7 + (1 * i)), _scoreManager.HiScores[i - 1].ToString());
            }
            _state = eGameState.HiScores;
        }

        public void MoveToReadQueuedLog()
        {
            if(!PlayerInputSystem.Replaying)
                _state = eGameState.ReadQueuedLog;
        }

        void ShowInventory(EntityCommandBuffer ecb)
        {
            Entities.WithAll<Disabled>().ForEach((Entity e) =>
            {
                if (EntityManager.HasComponent<InventoryUI>(e))
                {
                    ecb.RemoveComponent<Disabled>(e);
                }
            });

            populateInventory = true;
        }

        void PopulateInventory()
        {

            spriteRenderers.Clear();
            
            Entities.WithAll<InventoryItemUI>().ForEach((Entity e, ref Sprite2DRenderer spr) =>
            {
                spriteRenderers.Add(spr);
            });

            var invSys = EntityManager.World.GetExistingSystem<InventorySystem>();
            invSys.RenderInventoryItems(spriteRenderers);
            
            populateInventory = false;

        }

        void HideInventory(EntityCommandBuffer ecb)
        {
            Entities.WithAll<InventoryUI>().ForEach( e => { ecb.AddComponent<Disabled>(e, new Disabled()); });

        }

    }
}
