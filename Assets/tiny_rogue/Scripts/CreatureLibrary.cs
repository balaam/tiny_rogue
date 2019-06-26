using System;
using Unity.Entities;
using Unity.Tiny.Core2D;
using UnityEngine;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Color = Unity.Tiny.Core2D.Color;

namespace game
{
    public enum ECreatureId : int
    {
        Rat,
        Kobold,
        
        SpawnableCount,
        
        Player = SpawnableCount
    };
    
    public struct CreatureDescription
    {
        public string name;
        public int health;
        public int2 attackRange;
        public char ascii;
        public Color asciiColor;
    }
    
    
    
    public class CreatureLibrary
    {

        private EntityArchetype _creatureArcheType;

        public static CreatureDescription[] CreatureDescriptions = new[]
        {
            /* Rat */
            new CreatureDescription
            {
                name = "Rat", 
                health = 1, 
                attackRange = new int2(1,1),
                ascii = 'r', 
                asciiColor = new Color(0.9f, 0.5f, 0.3f)
            },
            /* Kobold */
            new CreatureDescription
            {
                name = "Kobold", 
                health = 3,
                attackRange = new int2(1,3),
                ascii = 'k',
                asciiColor = new Color(0.5f, 0.9f, 0.3f)
            },
            
            // Unspawnables
            /* Player */
            new CreatureDescription
            {
                name = "Player", 
                health = 10,
                attackRange = new int2(1,1),
                ascii = (char)1,
                asciiColor = new Color(1, 1, 1)
            },
        };

        public void Init(EntityManager em)
        {
            _creatureArcheType = em.CreateArchetype(new ComponentType[]
            {
                typeof(Parent),
                typeof(Translation),
                typeof(WorldCoord),
                typeof(Sprite2DRenderer),
                typeof(LayerSorting),  
                typeof(HealthPoints),
                typeof(BlockMovement),
                typeof(Creature),
                typeof(AttackStat),
                typeof(tag_Attackable)
            });
        }

        public Entity SpawnCreature(EntityManager entityManager, ECreatureId cId)
        {
            Entity entity = entityManager.CreateEntity(_creatureArcheType);
            CreatureDescription descr = CreatureDescriptions[(int) cId];
            
            Sprite2DRenderer s = new Sprite2DRenderer();
            LayerSorting l = new LayerSorting();
            Creature c = new Creature {id = (int)cId};
            HealthPoints hp = new HealthPoints {max = descr.health, now = descr.health};
            AttackStat att = new AttackStat { range = descr.attackRange };
            
            
            // Only tint sprites if ascii
            s.color = GlobalGraphicsSettings.ascii ? descr.asciiColor : Color.Default;
            s.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics(descr.ascii)];
            l.order = 1;
            
            entityManager.SetComponentData(entity, s);
            entityManager.SetComponentData(entity, c);
            entityManager.SetComponentData(entity, l);
            entityManager.SetComponentData(entity, hp);
            entityManager.SetComponentData(entity, att);
            return entity;
        }
        
        
        public Entity SpawnCreature(EntityCommandBuffer cb, ECreatureId cId)
        {
            Entity entity = cb.CreateEntity(_creatureArcheType);
            CreatureDescription descr = CreatureDescriptions[(int) cId];
            
            Sprite2DRenderer s = new Sprite2DRenderer();
            LayerSorting l = new LayerSorting();
            Creature c = new Creature {id = (int)cId};
            HealthPoints hp = new HealthPoints {max = descr.health, now = descr.health};
            AttackStat att = new AttackStat { range = descr.attackRange };
            
            
            // Only tint sprites if ascii
            s.color = GlobalGraphicsSettings.ascii ? descr.asciiColor : Color.Default;
            s.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics(descr.ascii)];
            l.order = 1;
            
            cb.SetComponent(entity, s);
            cb.SetComponent(entity, c);
            cb.SetComponent(entity, l);
            cb.SetComponent(entity, hp);
            cb.SetComponent(entity, att);
            return entity;
        }
    }

}
