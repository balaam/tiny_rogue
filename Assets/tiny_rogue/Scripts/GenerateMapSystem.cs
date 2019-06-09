using System;
using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Scenes;
using SceneData = Unity.Tiny.Scenes.SceneData;

namespace game
{
    public class GenerateMapSystem : ComponentSystem
    {


        bool _run = false;
        protected override void OnUpdate()
        {
            if (_run) return;

            Entity floorSprite = Entity.Null;
            Entity wallSprite = Entity.Null;
            Entity mapEntity = Entity.Null;
            Entity playerEntity = Entity.Null;
            
            int width = 0;
            int height = 0;

            bool foundMap = false;
            bool foundSpriteLookUp = false;
            bool foundPlayer = false;
            
            Entities.WithAll<Player, WorldCoord, Translation>().ForEach((Entity entity) =>
            {
                playerEntity = entity;
                foundPlayer = true;
            });
            
            Entities.ForEach((Entity entity, ref Viewport map) =>
            {
                mapEntity = entity;
                width = map.width;
                height = map.height;
                foundMap = true;
            });

            Entities.ForEach((ref SpriteLookUp lookUp) =>
            {
                floorSprite = lookUp.Dot;
                wallSprite = lookUp.Hash;
                foundSpriteLookUp = true;
            });

            // The scene may not be ready and have the map, player and sprites loaded.
            // If the required entities can't be founds early out and try again next frame.
            if (!(foundMap && foundSpriteLookUp && foundPlayer))
                return;
            
            var a = EntityManager.CreateArchetype(new ComponentType[]
            {
                typeof(Parent),
                typeof(Translation),
                typeof(WorldCoord),
                typeof(Rotation),
                typeof(Sprite2DRenderer),
                typeof(LayerSorting),
                typeof(Tile)
            });
            
            var startX = -(math.floor(width/2) * TinyRogueConstants.TileWidth);
            var startY = math.floor(height / 2) * TinyRogueConstants.TileHeight;

            WorldCoord playerWorldPos = EntityManager.GetComponentData<WorldCoord>(playerEntity);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Entity e = EntityManager.CreateEntity(a);
                    Sprite2DRenderer s = new Sprite2DRenderer();
                    Translation t = new Translation();
                    Parent p = new Parent();
                    WorldCoord c = new WorldCoord();
                    c.x = x;
                    c.y = y;
                    p.Value = mapEntity;
                    t.Value = new float3(
                        startX + (x * TinyRogueConstants.TileWidth), 
                        startY - (y * TinyRogueConstants.TileHeight), 0);
                    
                    s.color = new Unity.Tiny.Core2D.Color(1, 1, 1, 1);
                    
                    
                    // Border the level with a wall.
                    // Leaving room for status bar and log line.
                    bool isVWall = (x == 0 || x == width - 1) && y > 0 && y < height - 2;
                    bool isHWall = (y == 1 || y == height - 2);
                            
                    if(isVWall || isHWall)
                    {
                        s.sprite = wallSprite;    
                        EntityManager.AddComponent(e, typeof(BlockMovement));
                    }
                    else
                    {
                        s.sprite = floorSprite;
                    }

                    EntityManager.SetComponentData(e, s);
                    EntityManager.SetComponentData(e, t);
                    EntityManager.SetComponentData(e, p);
                    EntityManager.SetComponentData(e, c);

                    EntityManager.Instantiate(e);
                    
                    // Is this where the player is standing?
                    // Then move his location to that tile.
                    if (x == playerWorldPos.x && y == playerWorldPos.y)
                    {
                        EntityManager.SetComponentData(playerEntity, t);
                    }
                }
            }

            Console.WriteLine("Hello");
            _run = true;
        }
    }
}
