using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Color = Unity.Tiny.Core2D.Color;

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
            public int2 loc;
            public string text;
        }

        const int MaxLogHistory = 64;
        static readonly int2 DefaultLocation = new int2 { x = -1, y = -1 };

        List<LogEntry> _newLogs = new List<LogEntry>();
        List<LogEntry> _oldLogs = new List<LogEntry>();
        

        /// <summary>
        /// Add a log to be written on the top line
        /// Logs are displayed in the ordered they're added.
        /// </summary>
        /// <param name="log">The log line. Max length is View.Width (Default 80)</param>
        public void AddLog(string log)
        {
            var entry = new LogEntry { loc = DefaultLocation, text = log };
            _newLogs.Add(entry);
        }

        public void AddLog(int2 loc, string log)
        {
            var entry = new LogEntry { loc = loc, text= log };
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

        public void ShowNextLog(EntityCommandBuffer cb)
        {
            var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
            View view = gss.View;

            // Filter out events in the FOW
            for (int i = _newLogs.Count - 1; i >= 0; i--)
            {
                if (!_newLogs[i].loc.Equals(DefaultLocation))
                {
                    int idx = View.XYToIndex(_newLogs[i].loc, gss.View.Width);
                    Entity tileEntity = gss.View.ViewTiles[idx];
                    Tile tile = EntityManager.GetComponentData<Tile>(tileEntity);
                    if (!tile.IsSeen)
                    {
                        _newLogs.RemoveAt(i);
                    }
                }
            }

            if (_newLogs.Count > 0)
            {
                var gLog = EntityManager.World.GetOrCreateSystem<GraphicalLogSystem>();
                // Write the new log line out, remove it from the new list and add it to the old list.
                LogEntry topLog =  _newLogs[0];
                _newLogs.RemoveAt(0);
                _oldLogs.Add(topLog);
                if (GlobalGraphicsSettings.ascii)
                {
                    view.ClearLine(cb, 0, ' ');
                    view.Blit(cb, new int2(0, 0), topLog.text);
                }
                else
                {
                    gLog.AddToLog(topLog.text);
                }

                int blitEnd = topLog.text.Length + 1; // + 1 for the space
                
                string pageMsg = "[space]";
                if (HasQueuedLogs())
                {
                    LogEntry nextTopLog = _newLogs[0];

                    if (GlobalGraphicsSettings.ascii)
                    {
                        if ((blitEnd + nextTopLog.text.Length) < (view.Width - pageMsg.Length))
                        {
                            _newLogs.RemoveAt(0);
                            _oldLogs.Add(nextTopLog);
                            view.Blit(cb, new int2(blitEnd, 0), nextTopLog.text);
                        }
                        else
                        {

                            var pageXY = new int2(view.Width - pageMsg.Length, 0);
                            view.Blit(cb, pageXY, pageMsg, new Color(1, 1, 1, 1));
                            gss.MoveToReadQueuedLog();
                        }
                    }
                    else
                    {
                        _newLogs.RemoveAt(0);
                        _oldLogs.Add(nextTopLog);
                        var needsSpace = _newLogs.Count != 0 ? $" {pageMsg}" : "";
                        gLog.AddToLog($"{nextTopLog.text}{needsSpace}");
                        if (_newLogs.Count > 0)
                        {
                            gss.MoveToReadQueuedLog();
                        }
                    }
                }

                while (_oldLogs.Count > MaxLogHistory)
                    _oldLogs.RemoveAtSwapBack(_oldLogs.Count - 1);
            }
            else if (gss.IsInGame)
                view.ClearLine(cb, 0, ' ');
        }
        
        protected override void OnUpdate()
        {
            ShowNextLog(PostUpdateCommands);
        }
    }
}
