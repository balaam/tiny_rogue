using Unity.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace game
{
    
    [UpdateAfter(typeof(PlayerInputSystem))]
    public class TurnSystemGroup : ComponentSystemGroup { }
    
    [UpdateInGroup(typeof(TurnSystemGroup))]
    public class TurnManagementSystem : ComponentSystem
    {
        private List<TurnSystem> _turnSystems;
        private uint _turnCount = 0;
        public uint TurnCount
        {
            get { return _turnCount; }
        }
        
        public bool NeedToTickTurn { get; set; }

        public void ResetTurnCount()
        {
            _turnCount = 0;
        }

        public TurnManagementSystem()
        {
            _turnSystems = new List<TurnSystem>();
        }
        
        public void RegisterTurnSystem(TurnSystem system)
        {
#if DEBUG
            Assert.IsFalse(_turnSystems.Contains(system), "Trying to add a system to the turn manager more than once.");
#endif
            _turnSystems.Add(system);   
        }

        public void UnregisterTurnSystem(TurnSystem system)
        {
            if (_turnSystems.Contains(system))
            {
                _turnSystems.Remove(system);
            }
        }


        protected override void OnUpdate()
        {
            var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
            bool shouldTickSystems = gss.IsInGame && (NeedToTickTurn || _turnCount == 0);
            for (int i = 0; i < _turnSystems.Count; i++)
            {
                _turnSystems[i].Enabled = shouldTickSystems;
            }

            if (shouldTickSystems)
            {
                _turnCount++;
            }
            NeedToTickTurn = false;
        }

    }
}