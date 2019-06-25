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
            if (!EntityManager.World.GetExistingSystem<GameStateSystem>().IsInGame)
                return;

            Entities.WithAll<Sprite2DRenderer>().ForEach((Entity e, ref Sprite2DRenderer renderer, ref WorldCoord coord) =>
            {
                View view = EntityManager.World.GetExistingSystem<GameStateSystem>().View;
                if (view == null || view.ViewTiles == null)
                    return;

                if (EntityManager.HasComponent(e, typeof(Player)))
                {
                    renderer.color.a = TinyRogueConstants.DefaultColor.a;
                }
                else
                {
                    int tileIndex = View.XYToIndex(new int2(coord.x, coord.y), view.Width);
                    Entity tileEntity = view.ViewTiles[tileIndex];
                    Tile tile = EntityManager.GetComponentData<Tile>(tileEntity);

                    if (tile.IsSeen)
                        renderer.color.a = TinyRogueConstants.DefaultColor.a;
                    else if (tile.HasBeenRevealed && EntityManager.HasComponent(e, typeof(Tile)))
                        renderer.color.a = TinyRogueConstants.DefaultColor.a / 2;
                    else
                        renderer.color.a = 0;
                }
            });
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

            for (int x = math.max(0, startPosition.x - sightDepth); x < math.min(view.Width, startPosition.x + sightDepth); x++)
            {
                for (int y = math.max(0, startPosition.y - sightDepth);  y < math.min(view.Height, startPosition.y + sightDepth); y++)
                {
                    int2 endPos = new int2(x, y);
                    int2 currentPos = startPosition;

                    //while (currentPos.x != endPos.x && currentPos.y != endPos.y)
                    //{
                        currentPos = AStarPathfinding.getNextStep(currentPos, endPos);
                        Console.WriteLine(currentPos.x.ToString() + " " + currentPos.y.ToString());
                        int index = View.XYToIndex(currentPos, view.Width);
                        Console.WriteLine("Index: " + index);
                        if (index > 0 && index < view.ViewTiles.Length)
                        {
                            Entity tileEntity = view.ViewTiles[index];
                            Tile tile = EntityManager.GetComponentData<Tile>(tileEntity);

                            tile.IsSeen = true;
                            tile.HasBeenRevealed = true;

                            _revealedTiles.Add(tileEntity);

                            EntityManager.SetComponentData(tileEntity, tile);

                            if (EntityManager.HasComponent(tileEntity, typeof(BlockMovement)))
                                continue;
                        }
                        //else
                        //    continue;
                    //}
                }
            }
        }

        //private void CheckDirection(int2 position, int checkDepth, View view, Func<float, float> xOp, Func<float, float> yOp)
        //{

        //}
    }
}