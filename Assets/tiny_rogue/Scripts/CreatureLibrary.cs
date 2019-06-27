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
        public int sightRadius;
        public int speed;
        public int spriteId;
        public int ac;
    }

    public class CreatureLibrary
    {

        private EntityArchetype _creatureArchetype;
        private EntityArchetype _playerArchetype;

        public static CreatureDescription[] CreatureDescriptions = new[]
        {
            /* Fireskull */
            new CreatureDescription
            {
                name = "Fireskull", 
                health = 1, 
                attackRange = new int2(1,1),
                ascii = 'F', 
                asciiColor = new Color(0.9f, 0f, 0f),
                sightRadius = 10, // Fireskulls have good eyesight
                speed = 1,
                spriteId = 2,
                ac = 6
            },
            /* Kobold */
            new CreatureDescription
            {
                name = "Kobold", 
                health = 3,
                attackRange = new int2(1,3),
                ascii = 'k',
                asciiColor = new Color(0.5f, 0.9f, 0.3f),
                sightRadius = 3, // Kobolds aren't very good at spotting enemies
                speed = 3,
                spriteId = 1,
                ac = 9
            },
            
            // Unspawnables
            /* Player */
            new CreatureDescription
            {
                name = "Player", 
                health = TinyRogueConstants.StartPlayerHealth,
                attackRange = new int2(1,1),
                ascii = (char)1,
                asciiColor = new Color(1, 1, 1),
                spriteId = 0,
                ac = 13
            },
        };

        public void Init(EntityManager em)
        {
            _creatureArchetype = em.CreateArchetype(new ComponentType[]
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
                typeof(tag_Attackable),
                typeof(PatrollingState),
                typeof(MeleeAttackMovement),
                typeof(Sight),
                typeof(Speed),
                typeof(Mobile),
                typeof(Animated),
                typeof(Sprite2DSequencePlayer),
                typeof(NeedsAnimationStart),
                typeof(ArmorClass),
                typeof(TurnPriorityComponent)
            });
            
            _playerArchetype = em.CreateArchetype(new ComponentType[]
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
                typeof(tag_Attackable),
                typeof(Player),
                typeof(Level),
                typeof(ExperiencePoints),
                typeof(PlayerInput),
                typeof(GoldCount),
                typeof(InventoryComponent),
                typeof(Mobile),
                typeof(Animated),
                typeof(Sight),
                typeof(Speed),
                typeof(Sprite2DSequencePlayer),
                typeof(NeedsAnimationStart),
                typeof(ArmorClass),
                typeof(TurnPriorityComponent)
            });
        }
        
        public Entity SpawnCreature(EntityCommandBuffer cb, ECreatureId cId, int priority)
        {
            Entity entity = cb.CreateEntity(_creatureArchetype);
            CreatureDescription descr = CreatureDescriptions[(int) cId];
            
            Sprite2DRenderer s = new Sprite2DRenderer();
            LayerSorting l = new LayerSorting();
            Creature c = new Creature { id = (int)cId };
            HealthPoints hp = new HealthPoints { max = descr.health, now = descr.health };
            AttackStat att = new AttackStat { range = descr.attackRange };
            Sight sight = new Sight { SightRadius = descr.sightRadius };
            PatrollingState patrol = new PatrollingState();
            MeleeAttackMovement movement = new MeleeAttackMovement();
            Speed speed = new Speed { SpeedRate = descr.speed };
            Mobile mobile = new Mobile { Destination = new float3(0,0,0), Initial = new float3(0,0,0), MoveTime = 0,Moving = false };
            Animated animated = new Animated { Id = descr.spriteId, Direction = Direction.Right, Action = Action.None, AnimationTime = 0, AnimationTrigger = false};
            ArmorClass ac = new ArmorClass { AC = descr.ac };
            TurnPriorityComponent tp = new TurnPriorityComponent { Value = priority};

            // Only tint sprites if ascii
            s.color = GlobalGraphicsSettings.ascii ? descr.asciiColor : Color.Default;
            s.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics(descr.ascii)];
            l.layer = 2;
            
            cb.SetComponent(entity, s);
            cb.SetComponent(entity, c);
            cb.SetComponent(entity, l);
            cb.SetComponent(entity, hp);
            cb.SetComponent(entity, att);
            cb.SetComponent(entity, sight);
            cb.SetComponent(entity, movement);
            cb.SetComponent(entity, patrol);
            cb.SetComponent(entity, speed);
            cb.SetComponent(entity, mobile);
            cb.SetComponent(entity, animated);
            cb.SetComponent(entity, ac);

            return entity;
        }
        
        
        public Entity SpawnPlayer(EntityManager entityManager)
        {
            Entity entity = entityManager.CreateEntity(_playerArchetype);
            CreatureDescription descr = CreatureDescriptions[(int) ECreatureId.Player];
            
            Creature c = new Creature { id = (int) ECreatureId.Player };
            HealthPoints hp = new HealthPoints { max = descr.health, now = descr.health };
            AttackStat att = new AttackStat { range = descr.attackRange };
            Level lvl = new Level { level = 1 };
            ExperiencePoints exp = new ExperiencePoints { now = 0, next = LevelSystem.GetXPRequiredForLevel(1) };
            GoldCount gp = new GoldCount { count = 0 };
            Mobile mobile = new Mobile { Destination = new float3(0,0,0), Initial = new float3(0,0,0), MoveTime = 0, Moving = false };
            Animated animated = new Animated { Id = descr.spriteId, Direction = Direction.Right, Action = Action.None, AnimationTime = 0, AnimationTrigger = false };
            Sight sight = new Sight { SightRadius = 4 };
            ArmorClass ac = new ArmorClass {AC = descr.ac};
            TurnPriorityComponent tp = new TurnPriorityComponent { Value = -1};

            // Only tint sprites if ascii
            Sprite2DRenderer s = new Sprite2DRenderer();
            LayerSorting l = new LayerSorting { order = 2 };
            s.color = GlobalGraphicsSettings.ascii ? descr.asciiColor : Color.Default;
            s.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics(descr.ascii)];
            l.layer = 2;
            
            entityManager.SetComponentData(entity, s);
            entityManager.SetComponentData(entity, c);
            entityManager.SetComponentData(entity, l);
            entityManager.SetComponentData(entity, hp);
            entityManager.SetComponentData(entity, att);
            entityManager.SetComponentData(entity, lvl);
            entityManager.SetComponentData(entity, exp);
            entityManager.SetComponentData(entity, gp);
            entityManager.SetComponentData(entity, mobile);
            entityManager.SetComponentData(entity, animated);
            entityManager.SetComponentData(entity, sight);
            entityManager.SetComponentData(entity, ac);
            entityManager.SetComponentData(entity, tp);

            return entity;
        }

        public void ResetPlayer(EntityCommandBuffer cb, Entity player, WorldCoord worldCoord, Translation translation)
        {
            CreatureDescription descr = CreatureDescriptions[(int) ECreatureId.Player];
            
            Creature c = new Creature { id = (int) ECreatureId.Player };
            HealthPoints hp = new HealthPoints { max = descr.health, now = descr.health };
            AttackStat att = new AttackStat { range = descr.attackRange };
            Level lvl = new Level { level = 1 };
            ExperiencePoints exp = new ExperiencePoints {now = 0,  next = LevelSystem.GetXPRequiredForLevel(1)};
            GoldCount gp = new GoldCount { count = 0 };
            Mobile mobile = new Mobile { Destination = new float3(0,0,0), Initial = new float3(0,0,0), MoveTime = 0,Moving = false };
            Animated animated = new Animated { Id = descr.spriteId, Direction = Direction.Right, Action = Action.None, AnimationTime = 0, AnimationTrigger = false };
            Sight sight = new Sight { SightRadius = 4 };
            TurnPriorityComponent tp = new TurnPriorityComponent { Value = -1};
            
            // Only tint sprites if ascii
            Sprite2DRenderer s = new Sprite2DRenderer();
            LayerSorting l = new LayerSorting { order = 2 };
            s.color = GlobalGraphicsSettings.ascii ? descr.asciiColor : Color.Default;
            s.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics(descr.ascii)];
            l.layer = 2;
            
            cb.SetComponent(player, s);
            cb.SetComponent(player, c);
            cb.SetComponent(player, l);
            cb.SetComponent(player, hp);
            cb.SetComponent(player, att);
            cb.SetComponent(player, lvl);
            cb.SetComponent(player, exp);
            cb.SetComponent(player, gp);
            cb.SetComponent(player, mobile);
            cb.SetComponent(player, animated);
            cb.SetComponent(player, sight);
            cb.SetComponent(player, worldCoord);
            cb.SetComponent(player, translation);
            cb.SetComponent(player, tp);
        }
        
    }

}
