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

                GameStateSystem gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
                View view = gss.View;

                if (totalDistance <= viewDepth && !SightBlocked(playerPos, pos, view))
                {
                    tile.IsSeen = true;
                    tile.HasBeenRevealed = true;
                }
                else
                    tile.IsSeen = false;
            });
        }

        private bool SightBlocked(int2 start, int2 end, View view)
        {
            float x = (end - start).x;
            float y = (end - start).y;

            float xDirection = math.sign(x);
            float yDirection = math.sign(y);

            int2 currentTile = start;

            while (!currentTile.Equals(end))
            {
                int i = View.XYToIndex(currentTile, view.Width);
                Entity checkTile = view.ViewTiles[i];
                if (EntityManager.HasComponent(checkTile, typeof(BlockMovement)))
                    return true;

                if (currentTile.x != end.x)
                    currentTile.x += (int)xDirection;

                if (currentTile.y != end.y)
                    currentTile.y += (int)yDirection;
            }

            return false;
        }
    }
}