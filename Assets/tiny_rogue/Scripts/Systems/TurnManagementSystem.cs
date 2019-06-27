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
    
    [UpdateAfter(typeof(TurnSystemGroup))]
    public class DisplaySystemGroup : ComponentSystemGroup { }
    
    [UpdateAfter(typeof(TurnManagementSystem))]
    public class TurnSystemGroup : ComponentSystemGroup { }

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

        private TurnSystemGroup _tsg;
        private DisplaySystemGroup _dsg;

        public NativeQueue<ActionRequest> ActionQueue => _actionQueue;

        public uint TurnCount
        {
            get { return _turnCount; }
        }
        
        public bool NeedToTickTurn { get; set; }
        public bool NeedToTickDisplay { get; set; }

        public void ResetTurnCount()
        {
            _turnCount = 0;
            NeedToTickDisplay = true;
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
            NeedToTickDisplay = true;
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
            _tsg = EntityManager.World.GetOrCreateSystem<TurnSystemGroup>();
            _dsg = EntityManager.World.GetOrCreateSystem<DisplaySystemGroup>();
            _tsg.Enabled = false;
            _dsg.Enabled = true;
        }    

        protected override void OnDestroy()
        {
            if (_actionQueue.IsCreated)
            {
                _actionQueue.Dispose();
            }
            
            base.OnDestroy();
        }
        
        protected override void OnUpdate()
        {
            var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
            
            var fog = EntityManager.World.GetExistingSystem<FogOfWarSystem>();
            var gvs = EntityManager.World.GetExistingSystem<GameViewSystem>();
            var log = EntityManager.World.GetExistingSystem<LogSystem>();
            
            bool shouldTickSystems = gss.IsInGame && NeedToTickTurn;
            
            _tsg.Enabled = gss.IsInGame && NeedToTickTurn;
            _dsg.Enabled = gss.IsInGame && NeedToTickDisplay;
            if (_tsg.Enabled)
                _turnCount++;
            
            NeedToTickTurn = false;
            NeedToTickDisplay = false;
        }

    }
}