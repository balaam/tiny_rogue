using System;
using System.Collections.Generic;
using game;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using Unity.Tiny.Input;

namespace game
{
    public class LevelGenerationSystem : ComponentSystem
    {
        
        // this will probably become more complex
        private enum LevelType
        {
            DEFAULT,
            COMBAT
        };
        private NativeQueue<LevelType> queue = new NativeQueue<LevelType>(Allocator.Persistent);
        
        protected override void OnUpdate()
        {
            if (queue.Count == 0) return;

            var level = queue.Dequeue();
            var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();

            switch (level)
            {
                case LevelType.COMBAT:
                    GenerateCombatTestLevel(gss);
                    break;
                default:
                    GenerateLevel(gss);
                    break;
            }
        }

        private void GenerateEmptyLevel(GameStateSystem gss)
        {
            // Removing blocking tags from all tiles
            Entities.WithAll<BlockMovement, Tile>().ForEach((Entity entity) =>
            {
                PostUpdateCommands.RemoveComponent(entity, typeof(BlockMovement)); 
            });

            Entities.WithAll<Tile>().ForEach((Entity e, ref WorldCoord tileCoord, ref Sprite2DRenderer renderer) =>
            {
                var x = tileCoord.x;
                var y = tileCoord.y;
                
                bool isVWall = (x == 0 || x == gss.View.Width - 1) && y > 0 && y < gss.View.Height - 2;
                bool isHWall = (y == 1 || y == gss.View.Height - 2);
                
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

        private void GenerateLevel(GameStateSystem gss)
        {
            GenerateEmptyLevel(gss);
            
            // Hard code a couple of spear traps, so the player can die.
            var trap1Coord = new int2(12, 12);
            var trap2Coord = new int2(13, 11);
            gss.ArchetypeLibrary.CreateSpearTrap(EntityManager, trap1Coord, gss.View.ViewCoordToWorldPos(trap1Coord));
            gss.ArchetypeLibrary.CreateSpearTrap(EntityManager, trap2Coord, gss.View.ViewCoordToWorldPos(trap2Coord));

            var stairwayCoord = new int2(22, 15);
            gss.ArchetypeLibrary.CreateStairway(EntityManager, stairwayCoord, gss.View.ViewCoordToWorldPos(stairwayCoord));

            var crownCoord = new int2(13, 12);
            gss.ArchetypeLibrary.CreateCrown(EntityManager, crownCoord, gss.View.ViewCoordToWorldPos(crownCoord));
                                    
            PlacePlayer(gss);
        }

        private void PlacePlayer(GameStateSystem gss)
        {
            // Place the player
            Entities.WithAll<MoveWithInput>().ForEach((Entity player, ref WorldCoord coord, ref Translation translation, ref HealthPoints hp) =>
            {
                coord.x = 10;
                coord.y = 10;
                translation.Value = gss.View.ViewCoordToWorldPos(new int2(coord.x, coord.y));
                            
                hp.max = TinyRogueConstants.StartPlayerHealth;
                hp.now = hp.max;
            });

        }

        private void GenerateCombatTestLevel(GameStateSystem gss)
        {
            GenerateEmptyLevel(gss);
            PlacePlayer(gss);  

            // Create 'Exit'
            var crownCoord = new int2(1, 2);
            gss.ArchetypeLibrary.CreateCrown(EntityManager, crownCoord, gss.View.ViewCoordToWorldPos(crownCoord));
        }

        public void QueueGenerateLevel()
        {
            queue.Enqueue(LevelType.DEFAULT);
        }
        
        public void QueueCombatTestLevel()
        {
            queue.Enqueue(LevelType.COMBAT);
        }

    }
}