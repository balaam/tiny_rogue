using Unity.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace game
{
    public class TurnManager
    {
        static List<TurnSystem> _turnSystems = new List<TurnSystem>();
        uint _turnCount = 0;
        public uint TurnCount => _turnCount;
        public bool NeedToTickTurn { get; set; }

        public void ResetTurnCount()
        {
            _turnCount = 0;
        }
        
        public static void RegisterSystem(TurnSystem system)
        {
#if DEBUG
            Assert.IsFalse(_turnSystems.Contains(system), "Trying to add a system to the turn manager more than once.");
#endif
            _turnSystems.Add(system);   
        }

        public void OnTurn()
        {
            for (int i = 0; i < _turnSystems.Count; i++)
            {
                TurnManager._turnSystems[i].OnTurn(_turnCount);
            }
            _turnCount++;
            NeedToTickTurn = false;
        }
    }
}
