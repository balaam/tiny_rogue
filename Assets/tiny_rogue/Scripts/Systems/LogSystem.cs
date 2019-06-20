using Unity.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace game
{
    /// <summary>
    /// New logs should shown for one turn (feature to make this adjustable?)
    /// Logs should be queued if more than one is generated in turn
    /// A history of all messages shown should be possible.
    /// </summary>
    public class LogSystem : TurnSystem
    {
        class LogEntry
        {
            public string text;
        }

        const int MaxLogHistory = 64;

        List<LogEntry> _newLogs = new List<LogEntry>();
        List<LogEntry> _oldLogs = new List<LogEntry>();
        
        protected override void OnUpdate() { }

        /// <summary>
        /// Add a log to be written on the top line
        /// Logs are displayed in the ordered they're added.
        /// </summary>
        /// <param name="log">The log line. Max length is View.Width (Default 80)</param>
        public void AddLog(string log)
        {
            var entry = new LogEntry { text = log };
            _newLogs.Add(entry);
        }
        
        public override void OnTurn(uint turnNumber)
        {
            var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();

            if (_newLogs.Count > 0)
            {
                // Write the new log line out, remove it from the new list and add it to the old list.
                LogEntry topLog =  _newLogs[0];
                _newLogs.RemoveAtSwapBack(0);
                _oldLogs.Add(topLog);
                gss.View.ClearLine(EntityManager, 0, '.');
                gss.View.Blit(EntityManager, new int2(0,0), topLog.text);
                
                if(_oldLogs.Count > MaxLogHistory)
                    _oldLogs.RemoveAtSwapBack(_oldLogs.Count - 1);
            }
            else
                gss.View.ClearLine(EntityManager, 0, '.');
        }
    }
}
