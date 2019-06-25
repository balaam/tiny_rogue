using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Tiny.Core2D;

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

        public void Clear()
        {
            _newLogs.Clear();
            _oldLogs.Clear();
        }

        public bool HasQueuedLogs()
        {
            return _newLogs.Count > 0;
        }

        public void ShowNextLog()
        {
            var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
            View view = gss.View;

            if (_newLogs.Count > 0)
            {
                // Write the new log line out, remove it from the new list and add it to the old list.
                LogEntry topLog =  _newLogs[0];
                _newLogs.RemoveAt(0);
                _oldLogs.Add(topLog);
                view.ClearLine(EntityManager, 0, ' ');
                view.Blit(EntityManager, new int2(0,0), topLog.text);

                if (HasQueuedLogs())
                {
                    string pageMsg = "(cont)";
                    var pageXY = new int2(view.Width - pageMsg.Length, 0);
                    view.Blit(EntityManager, pageXY, pageMsg, new Color(1,1,1,1));
                }


                if(_oldLogs.Count > MaxLogHistory)
                    _oldLogs.RemoveAtSwapBack(_oldLogs.Count - 1);
            }
            else
                view.ClearLine(EntityManager, 0, ' ');
        }
        
        public override void OnTurn(uint turnNumber)
        {
            ShowNextLog();
        }
    }
}
