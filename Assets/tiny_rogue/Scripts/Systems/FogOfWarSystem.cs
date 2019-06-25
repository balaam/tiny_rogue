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

            Console.WriteLine("Player Position: " + startPosition.ToString());

            for (int x = startPosition.x - sightDepth; x < startPosition.x + sightDepth; x++)
            {
                for (int y = startPosition.y - sightDepth;  y < startPosition.y + sightDepth; y++)
                {
                    int2 endPos = new int2(x, y);
                    //int2 currentPos = startPosition;

                    AStarPathfinding.Path path = AStarPathfinding.getPath(startPosition, endPos);

                    List<int2> reversedPath = new List<int2>();
                    while (path.location.x != startPosition.x && path.location.y != startPosition.y)
                    {
                        reversedPath.Add(path.location);
                        path = path.stepFrom(path.location);
                    }
                    reversedPath.Reverse();

                    Console.WriteLine("\nStarting a new Path");
                    foreach (int2 position in reversedPath)
                        //while(path.location.x != endPos.x && path.location.y != endPos.y)
                        //while (currentPos.x != endPos.x && currentPos.y != endPos.y)
                    {
                        int2 currentPos = position; //AStarPathfinding.getNextStep(currentPos, endPos);

                        Console.WriteLine("Next Step: " + currentPos.ToString());

                        //path = path.stepFrom(path.location);

                        int index = View.XYToIndex(currentPos, view.Width);
                        Entity tileEntity = view.ViewTiles[index];
                        Tile tile = EntityManager.GetComponentData<Tile>(tileEntity);

                        tile.IsSeen = true;
                        tile.HasBeenRevealed = true;

                        _revealedTiles.Add(tileEntity);

                        EntityManager.SetComponentData(tileEntity, tile);

                        if (EntityManager.HasComponent(tileEntity, typeof(BlockMovement)))
                        {
                            Console.WriteLine("Break");
                            break;
                        }
                    }
                }
            }
        }
    }
}