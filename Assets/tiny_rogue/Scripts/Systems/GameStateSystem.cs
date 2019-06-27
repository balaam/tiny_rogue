using System;
using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Mathematics;
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
            DebugLevelSelect,
            HiScores,
            Inventory,
        }

        eGameState _state = eGameState.Startup;
        View _view = new View();
        ScoreManager _scoreManager = new ScoreManager();
        ArchetypeLibrary _archetypeLibrary = new ArchetypeLibrary();
        CreatureLibrary _creatureLibrary = new CreatureLibrary();
        private DungeonSystem _dungeon;

        private uint CurrentSeed = 1;
        public int CurrentLevel = 0;
        private int LastDungeonNumber;

        private uint MakeNewRandom()
        {
            return (uint)(Time.time * 100000.0);
        }

        public View View => _view;
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
            CurrentLevel++;
            CleanUpGameWorld(PostUpdateCommands);

            _dungeon.GenerateDungeon(PostUpdateCommands, _view, _creatureLibrary, _archetypeLibrary, CurrentLevel);

            // Apply doors
            foreach (var doorCoord in _dungeon.GetHorizontalDoors())
            {
                if (RandomRogue.Next(TinyRogueConstants.DoorProbability) == 0)
                {
                    _archetypeLibrary.CreateDoorway(EntityManager, doorCoord, _view.ViewCoordToWorldPos(doorCoord), true);
                }
            }
            foreach (var doorCoord in _dungeon.GetVerticalDoors())
            {
                if (RandomRogue.Next(TinyRogueConstants.DoorProbability) == 0)
                {
                    _archetypeLibrary.CreateDoorway(EntityManager, doorCoord, _view.ViewCoordToWorldPos(doorCoord), false);
                }
            }

            // Hard code a couple of spear traps, so the player can die.
            var trap1Coord = _dungeon.GetRandomPositionInRandomRoom();
            var trap2Coord = _dungeon.GetRandomPositionInRandomRoom();
            _archetypeLibrary.CreateSpearTrap(EntityManager, trap1Coord, _view.ViewCoordToWorldPos(trap1Coord));
            _archetypeLibrary.CreateSpearTrap(EntityManager, trap2Coord, _view.ViewCoordToWorldPos(trap2Coord));

            if (CurrentLevel != LastDungeonNumber)
            {
                var stairwayCoord = _dungeon.GetRandomPositionInRandomRoom();
                _archetypeLibrary.CreateStairway(EntityManager, stairwayCoord,
                    _view.ViewCoordToWorldPos(stairwayCoord));
            }
            else
            {
                var crownCoord = _dungeon.GetRandomPositionInRandomRoom();
                _archetypeLibrary.CreateCrown(EntityManager, crownCoord, _view.ViewCoordToWorldPos(crownCoord));
            }

            GenerateGold();

            GenerateCollectibles();


        }

        void GenerateCollectibles()
        {
            for (int i = 0; i < _dungeon.NumberOfCollectibles; i++)
             {
                 //TODO: figure out how it can know to avoid tiles that already have an entity
                 var collectibleCoord = _dungeon.GetRandomPositionInRandomRoom();
                 _archetypeLibrary.CreateCollectible(EntityManager, collectibleCoord, _view.ViewCoordToWorldPos(collectibleCoord));
             }
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

        static Unity.Tiny.Core2D.Color GetColorForTile(Tile tile)
        {
            Unity.Tiny.Core2D.Color color = TinyRogueConstants.DefaultColor;
            
            if (!tile.IsSeen && tile.HasBeenRevealed)
            {
                color.r /= 2f;
                color.g /= 2f;
                color.b /= 2f;
            }
            else if (!tile.IsSeen)
            {
                color.a = 0f;
            }

            return color;
        }
        
        private static Unity.Tiny.Core2D.Color GetColorForObject(Tile tile)
        {
            var color = TinyRogueConstants.DefaultColor;

            if (!tile.IsSeen)
            {
                color.a = 0f;
            }

            return color;
        }

        private void UpdateView(EntityCommandBuffer ecb)
        {
            var tileSprite = Sprite2DRenderer.Default;

            // Set all floor tiles
            tileSprite.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('.')];
            Entities.WithAll<Sprite2DRenderer, Floor>().ForEach((Entity e, ref Tile tile) =>
            {
                tileSprite.color = GetColorForTile(tile);
                ecb.SetComponent(e, tileSprite);
            });

            // Default all block tiles to a wall
            tileSprite.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('#')];
            Entities.WithAll<Sprite2DRenderer, Wall>().ForEach((Entity e, ref Tile tile) =>
            {
                tileSprite.color = GetColorForTile(tile);
                ecb.SetComponent(e, tileSprite);
            });
            
            
            var horizontalDoorOpen = Sprite2DRenderer.Default;
            horizontalDoorOpen.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('\\')]; 

            var verticalDoorOpen = Sprite2DRenderer.Default;
            verticalDoorOpen.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('/')];
            
            var horizontalDoorClosed = Sprite2DRenderer.Default;
            horizontalDoorClosed.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('_')];
            
            var verticalDoorClosed = Sprite2DRenderer.Default;
            verticalDoorClosed.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('|')];
            
            // Set all door tiles// horizontal // vertical
            // Set all closed door tiles // closed horizontal // closed vertical
            Entities.WithAll<Sprite2DRenderer, Door>().ForEach((Entity e, ref Door door, ref WorldCoord coord) =>
            {
                Sprite2DRenderer spriteRenderer;
                if (door.Opened)
                    spriteRenderer = door.Horizontal ? horizontalDoorOpen : verticalDoorOpen;
                else
                    spriteRenderer = door.Horizontal ? horizontalDoorClosed : verticalDoorClosed;
                
                // Check the tile, regardless of what entity we're looking at; this will tell objects if their tile is visible or not
                int tileIndex = View.XYToIndex(new int2(coord.x, coord.y), _view.Width);
                Entity tileEntity = _view.ViewTiles[tileIndex];
                Tile tile = EntityManager.GetComponentData<Tile>(tileEntity);
                spriteRenderer.color = GetColorForTile(tile);
                
                ecb.SetComponent(e, spriteRenderer);
            });
            
            // This reads from *before* the above changes, so shouldn't be used on the same entities
            Entities.WithNone<Player, Tile, Door>().ForEach(
                (Entity e, ref Sprite2DRenderer renderer, ref WorldCoord coord) =>
                {
                    Sprite2DRenderer spriteRenderer = renderer;

                    // Check the tile, regardless of what entity we're looking at; this will tell objects if their tile is visible or not
                    int tileIndex = View.XYToIndex(new int2(coord.x, coord.y), _view.Width);
                    Entity tileEntity = _view.ViewTiles[tileIndex];
                    Tile tile = EntityManager.GetComponentData<Tile>(tileEntity);

                    spriteRenderer.color.a = tile.IsSeen ? 1 : 0;

                    ecb.SetComponent(e, spriteRenderer);
                });
        }

        void GenerateGold()
       {
            // Saving the num in a variable so it can be used for
            // the replay system, if need be
            int goldPiles = RandomRogue.Next(10);
            for (int i = 0; i < goldPiles; i++)
            {
                //TODO: figure out how it can know to avoid tiles that already have an entity
                var goldCoord = _dungeon.GetRandomPositionInRandomRoom();
                _archetypeLibrary.CreateGold(EntityManager, goldCoord, _view.ViewCoordToWorldPos(goldCoord));
            }
       }

        public void GenerateCombatTestLevel()
        {
            _dungeon.GenerateDungeon(PostUpdateCommands, _view, _creatureLibrary, _archetypeLibrary, 1);

            for (int i = 0; i < 20; i++)
            {
                var worldCoord = _dungeon.GetRandomPositionInRandomRoom();
                var viewCoord = _view.ViewCoordToWorldPos(worldCoord);
                Entity ratEntity = _creatureLibrary.SpawnCreature(EntityManager, ECreatureId.Rat);

                EntityManager.SetComponentData(ratEntity, new WorldCoord {x = worldCoord.x, y = worldCoord.y});
                EntityManager.SetComponentData(ratEntity, new Translation {Value = viewCoord});
            }

            // Create 'Exit'
            var crownCoord = _dungeon.GetRandomPositionInRandomRoom();
            _archetypeLibrary.CreateCrown(EntityManager, crownCoord, _view.ViewCoordToWorldPos(crownCoord));
        }

        protected override void OnUpdate()
        {
            // Update the view when we're not in startup
            if(_state != eGameState.Startup)
                UpdateView(PostUpdateCommands);

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
                    CurrentLevel = 0;
                    if (input.GetKeyDown(KeyCode.D))
                    {
                        MoveToDebugLevelSelect(PostUpdateCommands);
                    }
                    else if(input.GetKeyDown(KeyCode.H))
                    {
                        MoveToHiScores(PostUpdateCommands);
                    }
                    else if (input.GetKeyUp(KeyCode.Space))
                    {
                        MoveToInGame(PostUpdateCommands, false);
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
                    if (input.GetKeyUp(KeyCode.Space))
                        MoveToTitleScreen(PostUpdateCommands);
                    else if (input.GetKeyUp(KeyCode.R))
                        MoveToInGame(PostUpdateCommands, true);
                } break;
                case eGameState.NextLevel:
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();
                    var log = EntityManager.World.GetExistingSystem<LogSystem>();

                    // Generate a new seed
                    CurrentSeed = MakeNewRandom();
                    GenerateLevel();
                    log.AddLog("You descend another floor.");
                    log.ShowNextLog(PostUpdateCommands); 
                    _state = eGameState.InGame;
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
                    else if (input.GetKeyUp(KeyCode.Space))
                    {
                        MoveToTitleScreen(PostUpdateCommands);
                    }
                } break;
                case eGameState.HiScores:
                {
                    var input = EntityManager.World.GetExistingSystem<InputSystem>();
                    if (input.GetKeyUp(KeyCode.Space))
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
            _dungeon.ClearDungeon(cb, _view);
            
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
            
            // Clear the screen.
            Entities.WithAll<Player>().ForEach((Entity Player, ref GoldCount gc, ref Level level) =>
            {
                _scoreManager.SetHiScores(gc.count + (level.level - 1) * 10);
                level.level = 1;
                gc.count = 0;
            });
            ClearView(cb);

            _view.Blit(cb, new int2(0, 0), "TINY ROGUE");
            _view.Blit(cb, new int2(30, 20),"PRESS SPACE TO BEGIN");
            _view.Blit(cb, new int2(30, 21), "PRESS H FOR HISCORES");
            _view.Blit(cb, new int2(70, 23),"(d)ebug");
            _state = eGameState.Title;
        }

        public void MoveToInGame( EntityCommandBuffer cb, bool replay )
        {
            
            // Generate a new seed
            if(!replay)
                CurrentSeed = MakeNewRandom();
            RandomRogue.Init(CurrentSeed);
            LastDungeonNumber = RandomRogue.Next(5, 10);

            var log = EntityManager.World.GetExistingSystem<LogSystem>();
            var tms = EntityManager.World.GetExistingSystem<TurnManagementSystem>();
            var pis = EntityManager.World.GetExistingSystem<PlayerInputSystem>();

            if( replay )
                pis.StartReplaying();
            else
                pis.StartRecording();

            GenerateLevel();
            tms.ResetTurnCount();
            log.AddLog("You enter the dungeon. (Use the arrow keys to explore!)");
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



        public void MoveToGameOver(EntityCommandBuffer cb)
        {
            CleanUpGameWorld(cb);
            _view.Blit(cb, new int2(0, 0), "GAME OVER!");
            _view.Blit(cb, new int2(30, 20),"PRESS SPACE TO TRY AGAIN");
            _view.Blit(cb, new int2(30, 21),"PRESS R FOR REPLAY");
            _state = eGameState.GameOver;
        }

        public void MoveToGameWin(EntityCommandBuffer cb)
        {
            CleanUpGameWorld(cb);
            _view.Blit(cb, new int2(0, 0), "YOU WIN!");
            _view.Blit(cb, new int2(30, 20),"PRESS SPACE TO START AGAIN");
            _view.Blit(cb, new int2(30, 21),"PRESS R FOR REPLAY");
            _state = eGameState.GameOver;
        }

        public void MoveToNextLevel(EntityCommandBuffer cb)
        {
            CleanUpGameWorld(cb);
            _view.Blit(cb, new int2(0, 0), "YOU FOUND STAIRS LEADING DOWN");
            _view.Blit(cb, new int2(30, 20), "PRESS SPACE TO CONTINUE");
            _state = eGameState.NextLevel;
        }

        public void MoveToHiScores(EntityCommandBuffer cb)
        {
            CleanUpGameWorld(cb);
            _view.Blit(cb, new int2(30, 7), "HiScores");
            _view.Blit(cb, new int2(25, 20), "Press Space to Continue");
            for (int i = 1; i < 11; i++)
            {
                _view.Blit(cb, new int2(30, 7 + (1 * i)), i.ToString() + ": ");
                _view.Blit(cb, new int2(35, 7 + (1 * i)), _scoreManager.HiScores[i - 1].ToString());
            }
            _state = eGameState.HiScores;
        }

        private void MoveToDebugLevelSelect(EntityCommandBuffer cb)
        {
            // Clear the screen.
            ClearView(cb);

            _view.Blit(cb, new int2(0, 0), "TINY ROGUE (Debug Levels)");
            _view.Blit(cb, new int2(30, 10),"1) Combat Test");
            _view.Blit(cb, new int2(30, 20),"PRESS SPACE TO EXIT");
            _state = eGameState.DebugLevelSelect;
        }

        public void MoveToReadQueuedLog()
        {
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

        }
        
        void HideInventory(EntityCommandBuffer ecb)
        {
            Entities.WithAll<InventoryUI>().ForEach( e => { ecb.AddComponent<Disabled>(e, new Disabled()); });

        }

    }
}
