using game;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using Unity.Tiny.Input;

namespace game
{
    public class LevelGenerationSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            // Does nothing... not the best System ever created
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

        public void GenerateLevel(GameStateSystem gss)
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
        }
        public void GenerateCombatTestLevel(GameStateSystem gss)
        {
            GenerateEmptyLevel(gss);
            
            // Place the player
            Entities.WithAll<MoveWithInput>().ForEach((Entity player, ref WorldCoord coord, ref Translation translation, ref HealthPoints hp) =>
            {
                coord.x = 10;
                coord.y = 10;
                translation.Value = gss.View.ViewCoordToWorldPos(new int2(coord.x, coord.y));
                            
                hp.max = TinyRogueConstants.StartPlayerHealth;
                hp.now = hp.max;
            });
            
            // Create 'Exit'
            var crownCoord = new int2(1, 2);
            gss.ArchetypeLibrary.CreateCrown(EntityManager, crownCoord, gss.View.ViewCoordToWorldPos(crownCoord));
        }
    }
}