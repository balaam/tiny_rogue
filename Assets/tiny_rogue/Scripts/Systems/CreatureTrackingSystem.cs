using System;
using System.Collections.Generic;
using Unity.Entities;

namespace game
{
    // This file is all very hacky but it's the quickest and easiest way to start tracking monster positions in ECS
    public class CreatureTrackingSystem: ComponentSystem
    {
        private uint lastTurn = 999999; // Kind of like -1 except not  
        private List<String> locations;

        public List<String> Locations => locations;
        
        protected override void OnUpdate()
        {
            var tms = EntityManager.World.GetOrCreateSystem<TurnManagementSystem>();
            if (tms.TurnCount != lastTurn)
            {
                lastTurn = tms.TurnCount;

                ResetCreatureMap();
                
                Entities.WithAll<Creature>().WithNone<Player>().ForEach((Entity creature, ref WorldCoord coord) =>
                    {
                        locations.Add(($"{coord.x},{coord.y}"));
                    });

            }
        }

        private void ResetCreatureMap()
        {
            locations = new List<String>(); // Probably no more than 100 monsters, right?
        }
    }
}