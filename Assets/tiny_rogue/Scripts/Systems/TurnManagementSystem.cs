using Unity.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using UnityEngine;
using UnityEngine.Assertions;

namespace game
{
    

    public struct ActionRequest
    {
        public Action Act;
        public Entity Ent;
        public uint2 Loc;
        public Direction Dir;
        public int Priority;
    };
    
    [UpdateAfter(typeof(PlayerInputSystem))]
    public class TurnManagementSystem : ComponentSystem
    {
        private List<ComponentSystemBase> _turnSystems;
        private uint _turnCount = 0;
        private NativeQueue<ActionRequest> _actionQueue;

        public NativeQueue<ActionRequest> ActionQueue => _actionQueue;

        public uint TurnCount
        {
            get { return _turnCount; }
        }
        
        public bool NeedToTickTurn { get; set; }

        public void ResetTurnCount()
        {
            _turnCount = 0;
        }

        public void AddActionRequest(Action a, Entity e, WorldCoord loc, Direction direction, int priority)
        {
            if (!_actionQueue.IsCreated)
            {
                _actionQueue = new NativeQueue<ActionRequest>(Allocator.TempJob);
            }
            ActionRequest ar;
            ar.Act = a;
            ar.Ent = e;
            ar.Loc = new uint2((uint)loc.x, (uint)loc.y);
            ar.Dir = direction;
            ar.Priority = priority;
            _actionQueue.Enqueue(ar);
        }

        public void AddPlayerActionRequest(Action a, Entity e, WorldCoord loc, Direction direction, int priority)
        {
            AddActionRequest(a, e, loc, direction, priority);
            NeedToTickTurn = true;
        }

        public void CleanActionQueue()
        {
            if (_actionQueue.IsCreated)
            {
                _actionQueue.Dispose();
            }
        }

        public TurnManagementSystem()
        {
            _turnSystems = new List<ComponentSystemBase>();
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            _actionQueue = new NativeQueue<ActionRequest>(Allocator.TempJob);
        }    

        protected override void OnDestroy()
        {
            if (_actionQueue.IsCreated)
            {
                _actionQueue.Dispose();
            }
            
            base.OnDestroy();
        }
        
        public void RegisterTurnSystem(ComponentSystemBase system)
        {
#if DEBUG
            Assert.IsFalse(_turnSystems.Contains(system), "Trying to add a system to the turn manager more than once.");
#endif
            _turnSystems.Add(system);
        }

        public void UnregisterTurnSystem(ComponentSystemBase system)
        {
            if (_turnSystems.Contains(system))
            {
                _turnSystems.Remove(system);
            }
        }


        protected override void OnUpdate()
        {
            var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
            
            var fog = EntityManager.World.GetExistingSystem<FogOfWarSystem>();
            var gvs = EntityManager.World.GetExistingSystem<GameViewSystem>();
            var log = EntityManager.World.GetExistingSystem<LogSystem>();
            
            bool shouldTickSystems = gss.IsInGame && NeedToTickTurn;
            for (int i = 0; i < _turnSystems.Count; i++)
            {
                // We must update these systems on the first turn to display the log, fog, and dungeon
                if (_turnCount == 0 && (_turnSystems[i] == fog || _turnSystems[i] == gvs || _turnSystems[i] == log))
                {
                    _turnSystems[i].Enabled = true;
                }
                else
                    _turnSystems[i].Enabled = shouldTickSystems;
            }
            
            // Step forward once but don't tick
            if(_turnCount == 0)
                _turnCount++;

            if (shouldTickSystems)
            {
                _turnCount++;
                Debug.Log($"New Turn {_turnCount}");
            }
            NeedToTickTurn = false;
        }

    }
}