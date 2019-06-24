using System;
using Unity.Entities;
using Unity.Tiny.Core2D;
using UnityEngine;
using Unity.Mathematics;

namespace game
{
    // Unity Tiny doesn't have good prefab support yet, so in the meantime just do it in code.
    public class ArchetypeLibrary
    {
        public EntityArchetype Tile { get; private set; }
        public EntityArchetype SpearTrap { get; private set; }
        public EntityArchetype Crown { get; private set; }
        public EntityArchetype  Stairway { get; private set; }
        public EntityArchetype  Gold { get; private set; }

        public void Init(EntityManager em)
        {
            Tile = em.CreateArchetype(new ComponentType[]
            {
                typeof(Parent),
                typeof(Translation),
                typeof(WorldCoord), // should be view coord?
                typeof(Sprite2DRenderer),
                typeof(LayerSorting),
                typeof(Tile)
            });

            SpearTrap = em.CreateArchetype(new ComponentType[]
            {
                typeof(Parent),
                typeof(Translation),
                typeof(WorldCoord), // should be view coord?
                typeof(Sprite2DRenderer),
                typeof(LayerSorting),  
                typeof(SpearTrap)
            });
            
            Crown = em.CreateArchetype(new ComponentType[]
            {
                typeof(Parent),
                typeof(Translation),
                typeof(WorldCoord), // should be view coord?
                typeof(Sprite2DRenderer),
                typeof(LayerSorting),  
                typeof(Crown)
            });

            Stairway = em.CreateArchetype(new ComponentType[]
            {
                typeof(Parent),
                typeof(Translation),
                typeof(WorldCoord), // should be view coord?
                typeof(Sprite2DRenderer),
                typeof(LayerSorting),
                typeof(Stairway)
            });

            Gold = em.CreateArchetype(new ComponentType[] //trying
            {                                             //to
                typeof(Parent),                           //avoid
                typeof(Translation),                      //any
                typeof(WorldCoord), // should be view coord? //merge
                typeof(Sprite2DRenderer),                 //conflicts
                typeof(LayerSorting),                     //hopefully
                typeof(Gold)
            });
        }

        public Entity CreateTile(EntityManager entityManager, int2 xy, float3 pos, Entity parent)
        {
            Entity entity = entityManager.CreateEntity(Tile);
            Sprite2DRenderer s = new Sprite2DRenderer();
            Translation t = new Translation();
            Parent p = new Parent();
            WorldCoord c = new WorldCoord(); // ViewCoord?
            p.Value = parent;
            t.Value = pos;

            c.x = xy.x;
            c.y = xy.y;
                
            s.color = new Unity.Tiny.Core2D.Color(1, 1, 1, 1);
            s.sprite = SpriteSystem.AsciiToSprite[' '];
                
            entityManager.SetComponentData(entity, s);
            entityManager.SetComponentData(entity, t);
            entityManager.SetComponentData(entity, p);
            entityManager.SetComponentData(entity, c);
            
            return entity;
        }

        public Entity CreateSpearTrap(EntityManager entityManager, int2 xy, float3 pos)
        {
            Entity entity = entityManager.CreateEntity(SpearTrap);
            
            Sprite2DRenderer s = new Sprite2DRenderer();
            Translation t = new Translation();
            WorldCoord c = new WorldCoord();
            LayerSorting l = new LayerSorting();
            t.Value = pos;
                
            c.x = xy.x;
            c.y = xy.y;
                
            s.color = new Unity.Tiny.Core2D.Color(1, 1, 1, 1);
            s.sprite = SpriteSystem.AsciiToSprite['^'];
            l.order = 1;
            
            entityManager.SetComponentData(entity, s);
            entityManager.SetComponentData(entity, t);
            entityManager.SetComponentData(entity, c);
            entityManager.SetComponentData(entity, l);
            
            return entity;
        }

        public Entity CreateCrown(EntityManager entityManager, int2 xy, float3 pos)
        {
            Entity entity = entityManager.CreateEntity(Crown);
            
            Sprite2DRenderer s = new Sprite2DRenderer();
            Translation t = new Translation();
            WorldCoord c = new WorldCoord();
            LayerSorting l = new LayerSorting();
            t.Value = pos;

            c.x = xy.x;
            c.y = xy.y;
            
            s.color = new Unity.Tiny.Core2D.Color(0.925f, 0.662f, 0.196f);
            s.sprite = SpriteSystem.AsciiToSprite[127];
            l.order = 1;
            
            entityManager.SetComponentData(entity, s);
            entityManager.SetComponentData(entity, t);
            entityManager.SetComponentData(entity, c);
            entityManager.SetComponentData(entity, l);
            
            return entity;
        }
        public Entity CreateStairway(EntityManager entityManager, int2 xy, float3 pos)
        {
            Entity entity = entityManager.CreateEntity(Stairway);

            Sprite2DRenderer s = new Sprite2DRenderer();
            Translation t = new Translation();
            WorldCoord c = new WorldCoord();
            LayerSorting l = new LayerSorting();
            t.Value = pos;

            c.x = xy.x;
            c.y = xy.y;

            s.color = new Unity.Tiny.Core2D.Color(18/255.0f, 222/255.0f, 23.0f/255.0f);
            s.sprite = SpriteSystem.AsciiToSprite['Z'];
            l.order = 1;

            entityManager.SetComponentData(entity, s);
            entityManager.SetComponentData(entity, t);
            entityManager.SetComponentData(entity, c);
            entityManager.SetComponentData(entity, l);

            return entity;
        }

        public Entity CreateGold(EntityManager entityManager, int2 xy, float3 pos)
        {
            // TODO: randomly spawn across level
            Entity entity = entityManager.CreateEntity(Gold);
            
            Sprite2DRenderer s = new Sprite2DRenderer();
            Translation t = new Translation();
            WorldCoord c = new WorldCoord();
            LayerSorting l = new LayerSorting();
            t.Value = pos;

            c.x = xy.x;
            c.y = xy.y;
            
            s.color = new Unity.Tiny.Core2D.Color(1, 0.5f, 0.2f);
            s.sprite = SpriteSystem.AsciiToSprite[236];
            l.order = 1;
            
            entityManager.SetComponentData(entity, s);
            entityManager.SetComponentData(entity, t);
            entityManager.SetComponentData(entity, c);
            entityManager.SetComponentData(entity, l);
            
            return entity;
        }
    }
}
