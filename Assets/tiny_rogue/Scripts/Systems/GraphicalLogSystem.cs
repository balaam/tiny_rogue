using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Text;
using UnityEngine;

namespace game
{
    public class GraphicalLogSystem : ComponentSystem
    {
        public const int MaxNumberOfLines = 3;
        bool _dirty = true;
        string _logString;
        string _oldLogString;
        string _evenOlderString;
        
        protected override void OnUpdate()
        {
            if (_dirty)
            {
                Debug.Log("Update log");
                _dirty = false;
                Entities.WithAll<LogEntry>().ForEach((Entity e, ref LogEntry entry) =>
                {
                    EntityManager.SetBufferFromString<TextString>(e, entry.Line == 1 
                        ? _logString 
                        : entry.Line == 2 
                            ? _oldLogString 
                            : _evenOlderString);
                });
            }
        }

        public void AddToLog(string log)
        {
            _dirty = true;
            _evenOlderString = _oldLogString;
            _oldLogString = _logString;
            _logString = log;
        }
    }
}
