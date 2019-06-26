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
        Kobold
    };
    
    public struct CreatureDescription
    {
        public string name;
        public int health;
        public char ascii;
        public Color asciiColor;
    }
    
    
    
    public class CreatureLibrary
    {

        private EntityArchetype _creatureArcheType;

        public static CreatureDescription[] CreatureDescriptions = new[]
        {
            /* Rat */
            new CreatureDescription {name = "Rat", health = 1, ascii = 'r', asciiColor = new Color(0.9f, 0.5f, 0.3f)},
            /* Kobold */
            new CreatureDescription {name = "Kobold", health = 3, ascii = 'k', asciiColor = new Color(0.5f, 0.9f, 0.3f)},
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
                typeof(tag_Attackable)
            });
        }

        public Entity SpawnCreature(EntityManager entityManager, ECreatureId cId)
        {
            Entity entity = entityManager.CreateEntity(_creatureArcheType);
            
            Sprite2DRenderer s = new Sprite2DRenderer();
            LayerSorting l = new LayerSorting();
            Creature c = new Creature {id = (int)cId};
            int maxHp = CreatureDescriptions[(int)cId].health;
            HealthPoints hp = new HealthPoints {max = maxHp, now = maxHp};
            
            // Only tint sprites if ascii
            s.color = GlobalGraphicsSettings.ascii ? CreatureDescriptions[(int)cId].asciiColor : Color.Default;
            s.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics(CreatureDescriptions[(int)cId].ascii)];
            l.order = 1;
            
            entityManager.SetComponentData(entity, s);
            entityManager.SetComponentData(entity, c);
            entityManager.SetComponentData(entity, l);
            entityManager.SetComponentData(entity, hp);
            return entity;
        }
        
        
        public Entity SpawnCreature(EntityCommandBuffer cb, ECreatureId cId)
        {
            Entity entity = cb.CreateEntity(_creatureArcheType);
            
            Sprite2DRenderer s = new Sprite2DRenderer();
            LayerSorting l = new LayerSorting();
            Creature c = new Creature {id = (int)cId};
            int maxHp = CreatureDescriptions[(int)cId].health;
            HealthPoints hp = new HealthPoints {max = maxHp, now = maxHp};
            
            // Only tint sprites if ascii
            s.color = GlobalGraphicsSettings.ascii ? CreatureDescriptions[(int)cId].asciiColor : Color.Default;
            s.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics(CreatureDescriptions[(int)cId].ascii)];
            l.order = 1;
            
            cb.SetComponent(entity, s);
            cb.SetComponent(entity, c);
            cb.SetComponent(entity, l);
            cb.SetComponent(entity, hp);
            return entity;
        }
    }

}
