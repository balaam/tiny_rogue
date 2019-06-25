using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;

namespace game
{
    public class FogOfWarSystem : ComponentSystem
    {
        private List<Entity> _revealedTiles = new List<Entity>();

        protected override void OnUpdate()
        {
        }

        private void ResetTilesInSight()
        {
            foreach (Entity e in _revealedTiles)
            {
                Tile tile = EntityManager.GetComponentData<Tile>(e);
                tile.IsSeen = false;
                EntityManager.SetComponentData(e, tile);
            }
        }

        public void CalculateFov(View view)
        {
            Entities.WithAll<Sight>().ForEach((Entity e, ref WorldCoord coord, ref Sight sight) =>
            {
                int2 worldPosition = new int2(coord.x, coord.y);
                RevealSeenTiles(worldPosition, sight.SightRadius, view);
            });
        }

        private void RevealSeenTiles(int2 startPosition, int sightDepth, View view)
        {
            ResetTilesInSight();

            for (int x = startPosition.x - sightDepth; x < startPosition.x + sightDepth; x++)
            {
                for (int y = startPosition.y - sightDepth;  y < startPosition.y + sightDepth; y++)
                {
                    int2 endPos = new int2(x, y);
                    SavedPath path = AStarPathfinding.getPath(startPosition, endPos).toSavedPath();

                    foreach (int2 currentPos in path.pathSteps)
                    {
                        int index = View.XYToIndex(currentPos, view.Width);
                        Entity tileEntity = view.ViewTiles[index];
                        Tile tile = EntityManager.GetComponentData<Tile>(tileEntity);

                        tile.IsSeen = true;
                        tile.HasBeenRevealed = true;

                        _revealedTiles.Add(tileEntity);

                        EntityManager.SetComponentData(tileEntity, tile);

                        if (EntityManager.HasComponent(tileEntity, typeof(BlockMovement)))
                            break;
                    }
                }
            }
        }
    }
}