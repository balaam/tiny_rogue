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
            int width = 0;
            int height = 0;

            bool foundMap = false;
            bool foundSpriteLookUp = false;
            
            
            Entities.ForEach((Entity entity, ref MapData map) =>
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

            // The scene may not be ready and have the map loaded.
            // If the map can't be founds early out and try again next frame.
            if (!(foundMap && foundSpriteLookUp))
                return;
            
            var a = EntityManager.CreateArchetype(new ComponentType[]
            {
                typeof(Parent),
                typeof(Translation),
                typeof(Rotation),
                typeof(Sprite2DRenderer),
                typeof(LayerSorting),
            });
            
            var startX = -(math.floor(width/2) * TinyRogueConstants.TileWidth);
            var startY = math.floor(height / 2) * TinyRogueConstants.TileHeight;

            for (int i = 0; i < width; i++)
            {
                for (int k = 0; k < height; k++)
                {
                    Entity e = EntityManager.CreateEntity(a);
                    Sprite2DRenderer s = new Sprite2DRenderer();
                    Translation t = new Translation();
                    Parent p = new Parent();
                    p.Value = mapEntity;
                    t.Value = new float3(
                        startX + (i * TinyRogueConstants.TileWidth), 
                        startY - (k * TinyRogueConstants.TileHeight), 0);
                    
                    s.color = new Unity.Tiny.Core2D.Color(1, 1, 1, 1);
                    
                    
                    // Border the level with a wall.
                    // Leaving room for status bar and log line.
                    bool isVWall = (i == 0 || i == width - 1) && k > 0 && k < height - 2;
                    bool isHWall = (k == 1 || k == height - 2);
                            
                    if(isVWall || isHWall)
                    {
                        s.sprite = wallSprite;                            
                        //this.world.addComponent(instance, BlockMovement);
                    }
                    else
                    {
                        s.sprite = floorSprite;
                    }


                    EntityManager.SetComponentData(e, s);
                    EntityManager.SetComponentData(e, t);
                    EntityManager.SetComponentData(e, p);

                    EntityManager.Instantiate(e);
                }
            }

            Console.WriteLine("Hello");
            _run = true;
        }
    }
}
