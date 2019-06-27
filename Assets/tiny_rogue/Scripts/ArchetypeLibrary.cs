using System;
using Unity.Entities;
using Unity.Tiny.Core2D;
using UnityEngine;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Color = Unity.Tiny.Core2D.Color;

namespace game
{
    // Unity Tiny doesn't have good prefab support yet, so in the meantime just do it in code.
    public class ArchetypeLibrary
    {
        public EntityArchetype Tile { get; private set; }
        public EntityArchetype SpearTrap { get; private set; }
        public EntityArchetype Crown { get; private set; }
        public EntityArchetype Stairway { get; private set; }
        public EntityArchetype Doorway { get; private set; }
        public EntityArchetype Collectible { get; private set; }
        public EntityArchetype Gold { get; private set; }
        public EntityArchetype HealingPotion { get; private set; }

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
            
            Doorway = em.CreateArchetype(new ComponentType[]
            {
                typeof(Parent),
                typeof(Translation),
                typeof(WorldCoord), // should be view coord?
                typeof(Sprite2DRenderer),
                typeof(LayerSorting),
                typeof(BlockMovement),
                typeof(Door)
            });

           Collectible = em.CreateArchetype(new ComponentType[]
            {
                typeof(Parent),
                typeof(Translation),
                typeof(WorldCoord),
                typeof(Sprite2DRenderer),
                typeof(LayerSorting),
                typeof(CanBePickedUp),
                typeof(Collectible)
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

            HealingPotion = em.CreateArchetype(new ComponentType[]
            {
                typeof(Parent),
                typeof(Translation),
                typeof(WorldCoord),
                typeof(Sprite2DRenderer),
                typeof(LayerSorting),
                typeof(HealItem)
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

            // Only tint sprites if ascii
            s.color = TinyRogueConstants.DefaultColor;
            if(GlobalGraphicsSettings.ascii)
                s.color.a = 0;

            s.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics(' ')];

            entityManager.SetComponentData(entity, s);
            entityManager.SetComponentData(entity, t);
            entityManager.SetComponentData(entity, p);
            entityManager.SetComponentData(entity, c);
            
            return entity;
        }

        public Entity CreateSpearTrap(EntityCommandBuffer ecb, int2 xy, float3 pos)
        {
            Entity entity = ecb.CreateEntity(SpearTrap);

            Sprite2DRenderer s = new Sprite2DRenderer();
            Translation t = new Translation();
            WorldCoord c = new WorldCoord();
            LayerSorting l = new LayerSorting();
            t.Value = pos;

            c.x = xy.x;
            c.y = xy.y;

            // Only tint sprites if ascii
            s.color = TinyRogueConstants.DefaultColor;
            if(GlobalGraphicsSettings.ascii)
                s.color.a = 0;
            s.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('^' )];
            l.order = 1;

            ecb.SetComponent(entity, s);
            ecb.SetComponent(entity, t);
            ecb.SetComponent(entity, c);
            ecb.SetComponent(entity, l);
            
            return entity;
        }

        public Entity CreateCrown(EntityCommandBuffer ecb, int2 xy, float3 pos)
        {
            Entity entity = ecb.CreateEntity(Crown);

            Sprite2DRenderer s = new Sprite2DRenderer();
            Translation t = new Translation();
            WorldCoord c = new WorldCoord();
            LayerSorting l = new LayerSorting();
            t.Value = pos;

            c.x = xy.x;
            c.y = xy.y;

            // Only tint sprites if ascii
            s.color = GlobalGraphicsSettings.ascii 
                ? new Unity.Tiny.Core2D.Color(0.925f, 0.662f, 0.196f) 
                : Color.Default;
            if(GlobalGraphicsSettings.ascii)
                s.color.a = 0;

            s.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics((char) 127 )];
            l.order = 1;

            ecb.SetComponent(entity, s);
            ecb.SetComponent(entity, t);
            ecb.SetComponent(entity, c);
            ecb.SetComponent(entity, l);
            
            return entity;
        }
        
        public Entity CreateStairway(EntityCommandBuffer ecb, int2 xy, float3 pos)
        {
            Entity entity = ecb.CreateEntity(Stairway);
            Sprite2DRenderer s = new Sprite2DRenderer();
            Translation t = new Translation();
            WorldCoord c = new WorldCoord();
            LayerSorting l = new LayerSorting();
            t.Value = pos;

            c.x = xy.x;
            c.y = xy.y;

            // Only tint sprites if ascii
            s.color = GlobalGraphicsSettings.ascii 
                ? new Unity.Tiny.Core2D.Color(18 / 255.0f, 222 / 255.0f, 23.0f / 255.0f) 
                : Color.Default;
            if(GlobalGraphicsSettings.ascii)
                s.color.a = 0;

            s.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('Z')];
            l.layer = 1;

            ecb.SetComponent(entity, s);
            ecb.SetComponent(entity, t);
            ecb.SetComponent(entity, c);
            ecb.SetComponent(entity, l);

            return entity;
        }
        
        public Entity CreateDoorway(EntityCommandBuffer ecb, int2 xy, float3 pos, bool horizontal)
        {
            Entity entity = ecb.CreateEntity(Doorway);

            Sprite2DRenderer s = new Sprite2DRenderer();
            Translation t = new Translation();
            WorldCoord c = new WorldCoord();
            LayerSorting l = new LayerSorting();
            Door d = new Door();
            d.Horizontal = horizontal;
            t.Value = pos;

            c.x = xy.x;
            c.y = xy.y;

            // Only tint sprites if ascii
            s.color = GlobalGraphicsSettings.ascii 
                ? new Unity.Tiny.Core2D.Color(18 / 255.0f, 222 / 255.0f, 23.0f / 255.0f) 
                : Color.Default;
            if(GlobalGraphicsSettings.ascii)
                s.color.a = 0;

            s.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics(horizontal ? '\\' : '/')];
            // Have to draw above character in graphical
            l.layer = (short)(GlobalGraphicsSettings.ascii ? 1 : 3);

            ecb.SetComponent(entity, s);
            ecb.SetComponent(entity, t);
            ecb.SetComponent(entity, c);
            ecb.SetComponent(entity, l);
            ecb.SetComponent(entity, d);

            return entity;
        }
        
        public void CreateCollectible(EntityCommandBuffer ecb, int2 xy, float3 pos)
        {
            Entity entity = ecb.CreateEntity(Collectible);

            Sprite2DRenderer s = new Sprite2DRenderer();
            Translation t = new Translation();
            WorldCoord c = new WorldCoord();
            LayerSorting l = new LayerSorting();
            CanBePickedUp p = new CanBePickedUp();
            HealthBonus hb = new HealthBonus();
            t.Value = pos;

            c.x = xy.x;
            c.y = xy.y;

            // Only tint sprites if ascii
            s.color = GlobalGraphicsSettings.ascii 
                ? new Unity.Tiny.Core2D.Color(1, 1, 1) 
                : Color.Default;
            if(GlobalGraphicsSettings.ascii)
                s.color.a = 0;

            l.layer = 1;

            p.appearance.color = s.color;
            p.appearance.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('S')];   //defaults
          
            p.name = new NativeString64("unknown pickup");
            p.description = new NativeString64("Check collectible gen");

            var collectibleGenSystem = World.Active.GetOrCreateSystem<CollectibleGenSystem>();
            
            collectibleGenSystem.GetRandomCollectible(ecb, entity, p, hb);
            s.sprite = p.appearance.sprite;

            ecb.SetComponent(entity, s);
            ecb.SetComponent(entity, t);
            ecb.SetComponent(entity, c);
            ecb.SetComponent(entity, l);
            ecb.SetComponent(entity, p);

        }

        public Entity CreateGold(EntityCommandBuffer ecb, int2 xy, float3 pos)
        {
            Entity entity = ecb.CreateEntity(Gold);
            
            Sprite2DRenderer s = new Sprite2DRenderer();
            Translation t = new Translation();
            WorldCoord c = new WorldCoord();
            LayerSorting l = new LayerSorting();
            t.Value = pos;

            c.x = xy.x;
            c.y = xy.y;
            
            // Only tint sprites if ascii
            s.color = GlobalGraphicsSettings.ascii 
                ? new Unity.Tiny.Core2D.Color(0.964f, 0.749f, 0.192f) 
                : Color.Default;
            if(GlobalGraphicsSettings.ascii)
                s.color.a = 0;

            s.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics((char) 236)];
            l.layer = 1;
            
            ecb.SetComponent(entity, s);
            ecb.SetComponent(entity, t);
            ecb.SetComponent(entity, c);
            ecb.SetComponent(entity, l);
            
            return entity;
        }

        public Entity CreateHealingItem(EntityCommandBuffer ecb, int2 xy, float3 pos, int healAmount)
        {
            Entity entity = ecb.CreateEntity(HealingPotion);

            HealItem heal = new HealItem();
            Sprite2DRenderer s = new Sprite2DRenderer();
            Translation t = new Translation();
            WorldCoord c = new WorldCoord();
            LayerSorting l = new LayerSorting();
            t.Value = pos;

            c.x = xy.x;
            c.y = xy.y;

            // Only tint sprites if ascii
            s.color = GlobalGraphicsSettings.ascii
                ? new Unity.Tiny.Core2D.Color(1.0f, 0.26f, 0.23f)
                : Color.Default;
            if (GlobalGraphicsSettings.ascii)
                s.color.a = 0;

            s.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics((char)235)];
            l.layer = 1;

            heal.HealAmount = healAmount;

            ecb.SetComponent(entity, s);
            ecb.SetComponent(entity, t);
            ecb.SetComponent(entity, c);
            ecb.SetComponent(entity, l);
            ecb.SetComponent(entity, heal);

            return entity;
        }
    }
}
