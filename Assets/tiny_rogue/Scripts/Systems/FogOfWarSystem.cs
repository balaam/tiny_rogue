using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;

namespace game
{
    public class FogOfWarSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            if (!EntityManager.World.GetExistingSystem<GameStateSystem>().IsInGame)
                return;

            int2 playerPos = int2.zero;
            int viewDepth = 0;

            Entities.WithAll<Player>().ForEach((Entity e, ref WorldCoord coord, ref Sight sight) =>
            {
                playerPos = new int2(coord.x, coord.y);
                viewDepth = sight.SightRadius;
            });

            Entities.WithAll<Tile>().ForEach((Entity e, ref Tile tile, ref WorldCoord coord) =>
            {
                int2 pos = new int2(coord.x, coord.y);
                float totalDistance = math.sqrt(math.pow(math.distance(playerPos.x, pos.x), 2) +
                                                math.pow(math.distance(playerPos.y, pos.y), 2));

                if (totalDistance <= viewDepth)
                {
                    tile.IsSeen = true;
                    tile.HasBeenRevealed = true;
                }
                else
                    tile.IsSeen = false;
            });
        }
    }
}