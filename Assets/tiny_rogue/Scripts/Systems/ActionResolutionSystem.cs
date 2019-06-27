using System;
using System.Security.Permissions;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using UnityEngine;

namespace game
{

    [UpdateAfter(typeof(TurnManagementSystem))]
    public class TurnSystemGroup : ComponentSystemGroup { }

    public enum EInteractionFlags : byte
    {
        None = 0,
        Interact = 1 << 3,
        Door = 1 << 4,
        Hostile = 1 << 5,
        Player = 1 << 6,
        Blocking = 1 << 7,
    }

    public struct PendingMove
    {
        public Direction Dir;
        public Entity Ent;
        public WorldCoord Wc;
    }

    public struct PendingWait
    {
        public Direction Dir;
        public Entity Ent;
        public bool Ouch;
        //public WorldCoord Wc;
    }


    public struct PendingAttack
    {
        public Entity Attacker;
        public Direction AttackerDir;
        public Entity Defender;
        public int2 LogLoc;
    }

    public struct PendingDoorOpen
    {
        public Entity DoorEnt;
        public Entity OpeningEntity;
    }

    public struct PendingInteractions
    {
        public Direction Dir;
        public uint2 InteractPos;
        public Entity InteractingEntity;
    }

    [UpdateInGroup(typeof(TurnSystemGroup))]
    public class ActionResolutionSystem : JobComponentSystem
    {
        private NativeArray<byte> _flagMap;
        private NativeArray<Entity> _entityMap;
        private int2 _cachedMapSize;
        private GameStateSystem _gss;
        private TurnManagementSystem _tms;
        private EntityQuery _mapFillQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            _gss = EntityManager.World.GetOrCreateSystem<GameStateSystem>();
            _tms = EntityManager.World.GetOrCreateSystem<TurnManagementSystem>();

            ResizeMaps(_gss.View.Width, _gss.View.Height);
            var query = new EntityQueryDesc
            {
                All = new ComponentType[] {ComponentType.ReadOnly<BlockMovement>(), ComponentType.ReadOnly<WorldCoord>()}

            };

            _mapFillQuery = GetEntityQuery(query);
        }

        protected override void OnDestroy()
        {
            if (_entityMap.IsCreated) { _entityMap.Dispose();}
            if (_flagMap.IsCreated) { _flagMap.Dispose();}
            base.OnDestroy();
        }

        private void ResizeMaps(int width, int height)
        {
            // Empty all maps
            if (_flagMap.IsCreated) { _flagMap.Dispose(); }
            if (_entityMap.IsCreated) { _entityMap.Dispose(); }

            // Resize
            int mapSize = width * height;
            _flagMap = new NativeArray<byte>(mapSize, Allocator.Persistent, NativeArrayOptions.ClearMemory);
            _entityMap = new NativeArray<Entity>(mapSize, Allocator.Persistent, NativeArrayOptions.ClearMemory);

            // Cache new size
            _cachedMapSize.x = width;
            _cachedMapSize.y = height;
        }

        struct ClearMapsJob : IJob
        {
            public NativeArray<byte> FlagsMap;
            public NativeArray<Entity> EntityMap;

            public void Execute()
            {
                for (var i = 0; i < FlagsMap.Length; i++)
                {
                    FlagsMap[i] = 0;
                }
                for (var i = 0; i < EntityMap.Length; i++)
                {
                    EntityMap[i] = Entity.Null;
                }
            }
        }

        struct FillMapsJob : IJobChunk
        {
            public int2 MapSize;
            public NativeArray<byte> FlagsMap;
            public NativeArray<Entity> EntityMap;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public ArchetypeChunkComponentType<WorldCoord> WorldCoordType;
            [ReadOnly] public ArchetypeChunkComponentType<BlockMovement> BlockedMovementType;
            [ReadOnly] public ArchetypeChunkComponentType<Door> DoorType;
            [ReadOnly] public ArchetypeChunkComponentType<tag_Attackable> HostileType;
            [ReadOnly] public ArchetypeChunkComponentType<Player> PlayerType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkhWorldCoords = chunk.GetNativeArray(WorldCoordType);
                var entities = chunk.GetNativeArray(EntityType);
                byte flags = (byte)EInteractionFlags.None;
                flags |= (byte)(chunk.Has<BlockMovement>(BlockedMovementType) ? EInteractionFlags.Blocking : EInteractionFlags.None);
                flags |= (byte)(chunk.Has<Door>(DoorType) ? EInteractionFlags.Door : EInteractionFlags.None);
                flags |= (byte)(chunk.Has<tag_Attackable>(HostileType) ? EInteractionFlags.Hostile : EInteractionFlags.None);
                flags |= (byte)(chunk.Has<Player>(PlayerType) ? EInteractionFlags.Player : EInteractionFlags.None);

                for (var i = 0; i < chunk.Count; i++)
                {
                    var worldCoord = chunkhWorldCoords[i];
                    var idx = worldCoord.x + worldCoord.y * MapSize.x;
                    if (flags > FlagsMap[idx])
                    {
                        FlagsMap[idx] = flags;
                        EntityMap[idx] = entities[i];
                    }
                }
            }
        }

        struct ConsumeActionsJob : IJob
        {
            public int2 MapSize;
            public NativeQueue<ActionRequest> ActionQueue;
            public NativeArray<byte> FlagsMap;
            public NativeArray<Entity> EntityMap;
            public NativeQueue<PendingMove> PendingMoves;
            public NativeQueue<PendingWait> PendingWaits;
            public NativeQueue<PendingAttack> PendingAttacks;
            public NativeQueue<PendingDoorOpen> PendingOpens;
            public NativeQueue<PendingInteractions> PendingInteractions;

            private int GetIndex(uint2 loc)
            {
                return (int) loc.x + (int) loc.y * MapSize.x;
            }

            void TryMove(Entity e, Direction direction, uint2 moveFrom, uint2 moveTo)
            {
                int moveToIdx = GetIndex(moveTo);
                int moveFromIdx = GetIndex(moveFrom);
                byte targetFlags = FlagsMap[moveToIdx];
                if ((targetFlags & (byte) EInteractionFlags.Blocking) == 0)
                {
                    PendingMoves.Enqueue(new PendingMove
                        { Ent = e, Dir = direction, Wc = new WorldCoord { x = (int) moveTo.x, y = (int) moveTo.y } });
                    FlagsMap[moveToIdx] = FlagsMap[moveFromIdx];
                    EntityMap[moveToIdx] = EntityMap[moveFromIdx];
                    FlagsMap[moveFromIdx] = 0;
                    EntityMap[moveFromIdx] = Entity.Null;
                }
                if ((targetFlags & (byte) EInteractionFlags.Blocking) != 0 && (targetFlags & (byte) EInteractionFlags.Door) == 0 && (targetFlags & ((byte) EInteractionFlags.Hostile)) == 0)
                {
                    PendingWaits.Enqueue(new PendingWait { Ent = e, Dir = direction, Ouch = true }); //Don't move
                }
                else if ((targetFlags & ((byte) EInteractionFlags.Hostile | (byte) EInteractionFlags.Player)) != 0)
                {
                    PendingAttacks.Enqueue(new PendingAttack { Attacker = e, AttackerDir = direction, Defender = EntityMap[moveToIdx], LogLoc = new int2 {x = (int)moveFrom.x, y = (int)moveFrom.y }});
                }
                else if ((targetFlags & (byte) EInteractionFlags.Door) != 0)
                {
                    PendingOpens.Enqueue(new PendingDoorOpen { DoorEnt = EntityMap[moveToIdx], OpeningEntity = e });
                    FlagsMap[moveToIdx] &= (byte)~(EInteractionFlags.Blocking);
                }

            }

            public void Execute()
            {
                if (ActionQueue.IsCreated)
                {
                    ActionRequest ar;
                    while (ActionQueue.TryDequeue(out ar))
                    {
                        switch (ar.Act)
                        {
                            case Action.MoveUp:
                            {
                                uint2 moveTo = ar.Loc;
                                moveTo.y -= 1;
                                TryMove(ar.Ent, Direction.Right, ar.Loc, moveTo);
                            } break;

                            case Action.MoveDown:
                            {
                                uint2 moveTo = ar.Loc;
                                moveTo.y += 1;
                                TryMove(ar.Ent, Direction.Left, ar.Loc, moveTo);
                            } break;

                            case Action.MoveLeft:
                            {
                                uint2 moveTo = ar.Loc;
                                moveTo.x -= 1;
                                TryMove(ar.Ent, Direction.Left, ar.Loc, moveTo);
                            } break;

                            case Action.MoveRight:
                            {
                                uint2 moveTo = ar.Loc;
                                moveTo.x += 1;
                                TryMove(ar.Ent, Direction.Right, ar.Loc, moveTo);
                            } break;

                            case Action.Interact:
                            {
                                PendingInteractions.Enqueue(
                                    new PendingInteractions()
                                    {
                                        Dir = ar.Dir,
                                        InteractingEntity = ar.Ent,
                                        InteractPos = ar.Loc
                                    });
                            } break;

                            case Action.Wait:
                            {
                                PendingWaits.Enqueue(new PendingWait { Ent = ar.Ent, Dir = ar.Dir, Ouch = false });
                            } break;
                        }
                    }
                }
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            if (_cachedMapSize.x != _gss.View.Width || _cachedMapSize.y != _gss.View.Height)
            {
                ResizeMaps(_gss.View.Width, _gss.View.Height);
            }

            var clearJob = new ClearMapsJob()
            {
                FlagsMap = _flagMap,
                EntityMap = _entityMap
            };

            var clearJobHandle = clearJob.Schedule(inputDependencies);

            var fillJob = new FillMapsJob()
            {
                MapSize = _cachedMapSize,
                FlagsMap = _flagMap,
                EntityMap = _entityMap,
                EntityType = GetArchetypeChunkEntityType(),
                WorldCoordType = GetArchetypeChunkComponentType<WorldCoord>(true),
                BlockedMovementType = GetArchetypeChunkComponentType<BlockMovement>(true),
                DoorType = GetArchetypeChunkComponentType<Door>(true),
                HostileType = GetArchetypeChunkComponentType<tag_Attackable>(true),
                PlayerType = GetArchetypeChunkComponentType<Player>(true)
            };
            var fillJobHandle = fillJob.Schedule(_mapFillQuery, clearJobHandle);

            var pendingMoves = new NativeQueue<PendingMove>(Allocator.TempJob);
            var pendingWaits = new NativeQueue<PendingWait>(Allocator.TempJob);
            var pendingAttacks = new NativeQueue<PendingAttack>(Allocator.TempJob);
            var pendingOpens = new NativeQueue<PendingDoorOpen>(Allocator.TempJob);
            var pendingInteractions = new NativeQueue<PendingInteractions>(Allocator.TempJob);

            var actionJob = new ConsumeActionsJob()
            {
                MapSize = _cachedMapSize,
                ActionQueue = _tms.ActionQueue,
                FlagsMap = _flagMap,
                EntityMap = _entityMap,
                PendingMoves = pendingMoves,
                PendingWaits = pendingWaits,
                PendingAttacks = pendingAttacks,
                PendingOpens = pendingOpens,
                PendingInteractions =  pendingInteractions
            };
            var actionJobHandle =actionJob.Schedule(fillJobHandle);

            actionJobHandle.Complete();

            // TODO: Jobify?
            var log = EntityManager.World.GetExistingSystem<LogSystem>();
            PendingMove pm;
            while (pendingMoves.TryDequeue(out pm))
            {
                var trans = _gss.View.ViewCoordToWorldPos(new int2(pm.Wc.x, pm.Wc.y));
                if (GlobalGraphicsSettings.ascii)
                {
                    EntityManager.SetComponentData(pm.Ent, new Translation { Value = trans });
                }
                else
                {
                    var anim = EntityManager.World.GetExistingSystem<AnimationSystem>();
                    var mobile = EntityManager.GetComponentData<Mobile>(pm.Ent);
                    mobile.Initial = EntityManager.GetComponentData<Translation>(pm.Ent).Value;
                    mobile.Destination = trans;
                    EntityManager.SetComponentData(pm.Ent, mobile);
                    anim.StartAnimation(pm.Ent, Action.Move, pm.Dir);
                }
                EntityManager.SetComponentData(pm.Ent, pm.Wc);
            }

            PendingWait pw;
            while (pendingWaits.TryDequeue(out pw))
            {
                if (pw.Ouch && EntityManager.HasComponent<Player>(pw.Ent))
                {
                    log.AddLog("You bumped into a wall. Ouch.");
                }
                
                var anim = EntityManager.World.GetExistingSystem<AnimationSystem>();
                anim.StartAnimation(pw.Ent, pw.Ouch ? Action.None :Action.Wait, pw.Dir);
            }

            PendingAttack pa;
            while (pendingAttacks.TryDequeue(out pa))
            {
                AttackStat att = EntityManager.GetComponentData<AttackStat>(pa.Attacker);
                Creature attacker = EntityManager.GetComponentData<Creature>(pa.Attacker);
                HealthPoints hp = EntityManager.GetComponentData<HealthPoints>(pa.Defender);
                Creature defender = EntityManager.GetComponentData<Creature>(pa.Defender);
                HealthPoints attackerHp = EntityManager.GetComponentData<HealthPoints>(pa.Attacker);

                if (attackerHp.now <= 0) // don't let the dead attack, is this hack? Maybe.
                    continue;

                int dmg = RandomRogue.Next(att.range.x, att.range.y);
                bool firstHit = hp.now == hp.max;

                hp.now -= dmg;

                var anim = EntityManager.World.GetExistingSystem<AnimationSystem>();
                anim.StartAnimation(pa.Attacker, Action.Attack, pa.AttackerDir);

                string attackerName = CreatureLibrary.CreatureDescriptions[attacker.id].name;
                string defenderName = CreatureLibrary.CreatureDescriptions[defender.id].name;

                bool playerAttack = attackerName == "Player";
                bool killHit = hp.now <= 0;

                string logStr;

                if (playerAttack && killHit && firstHit)
                {
                    logStr = string.Concat("You destroy the ", defenderName);
                    logStr = string.Concat(logStr, ".");
                    ExperiencePoints xp = EntityManager.GetComponentData<ExperiencePoints>(pa.Attacker);
                    xp.now += hp.max; //XP awarded equals the defenders max hp
                    EntityManager.SetComponentData(pa.Attacker, xp);
                }
                else if (playerAttack)
                {
                    logStr = string.Concat(string.Concat(string.Concat(string.Concat(
                                    "You hit the ",
                                    defenderName),
                                    " for "),
                                    dmg.ToString()),
                                    " damage!");

                    if(killHit)
                    {
                        logStr = string.Concat(logStr, " Killing it.");
                        ExperiencePoints xp = EntityManager.GetComponentData<ExperiencePoints>(pa.Attacker);
                        xp.now += hp.max; //XP awarded equals the defenders max hp
                        EntityManager.SetComponentData(pa.Attacker, xp);
                    }
                }
                else
                {
                	if (defenderName == "Player")
                        defenderName = "you";

                    logStr = string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(
                                    attackerName,
                                    " hits "),
                                    defenderName),
                                    " for "),
                                    dmg.ToString()),
                                    " damage!");
                }
                log.AddLog(pa.LogLoc,logStr);

                EntityManager.SetComponentData(pa.Defender, hp);
            }

            PendingDoorOpen pd;
            while (pendingOpens.TryDequeue(out pd))
            {
                if (EntityManager.HasComponent<Player>(pd.OpeningEntity))
                {
                    log.AddLog("You opened a door.");
                }
                Sprite2DRenderer s = EntityManager.GetComponentData<Sprite2DRenderer>(pd.DoorEnt);
                var door = EntityManager.GetComponentData<Door>(pd.DoorEnt);
                door.Locked = false;
                door.Opened = true;
                EntityManager.RemoveComponent(pd.DoorEnt, typeof(BlockMovement));
                EntityManager.SetComponentData(pd.DoorEnt, door);
                EntityManager.SetComponentData(pd.DoorEnt, s);
            }

            PendingInteractions pi;
            while (pendingInteractions.TryDequeue(out pi))
            {
                using (var entities = EntityManager.GetAllEntities(Allocator.TempJob))
                {
                    foreach (Entity e in entities)
                    {
                        if (EntityManager.HasComponent(e, typeof(WorldCoord)) &&
                            EntityManager.HasComponent(e, typeof(Stairway)))
                        {
                            WorldCoord coord = EntityManager.GetComponentData<WorldCoord>(e);
                            int2 ePos = new int2(coord.x, coord.y);

                            if (pi.InteractPos.x == ePos.x && pi.InteractPos.y == ePos.y)
                            {
                                if (EntityManager.HasComponent(e, typeof(Stairway)))
                                    _gss.MoveToNextLevel(new EntityCommandBuffer(Allocator.TempJob));
                            }
                        }
                    }
                }
            }

            // Cleanup
            pendingMoves.Dispose();
            pendingAttacks.Dispose();
            pendingWaits.Dispose();
            pendingOpens.Dispose();
            pendingInteractions.Dispose();

            return actionJobHandle;
        }
    }
}
