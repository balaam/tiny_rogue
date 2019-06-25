using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Tiny.Core2D;

namespace game
{
    public class AStarPathfinding
    {
        private class LocationRef
        {
            // Tile coordinates
            public int2 location;
            public int totalPath;
            public int distanceFromStart;
            public int distanceToDestination; // this is an estimated value
            public LocationRef Parent;
        }

        public static int2 getPath(int2 start, int2 end)
        {
            LocationRef current = null;
            var startLoc = new LocationRef {location = start};
            var closedList = new List<LocationRef>();
            var openList = new LinkedList<LocationRef>();
            var g = 0;

            openList.AddLast(startLoc);
            while (openList.Count > 0)
            {
                current = minList(openList);
                openList.Remove(current);
                closedList.Add(current);
                // Did we just find the end square? If so then we're finished!
                if (current.location.x == end.x && current.location.y == end.y) break;
                var adjacentSquares = getAdjacentSquares(start);
                foreach (var adjacentSquare in adjacentSquares)
                {
                    // If we've already thoroughly searched this square then do nothing
                    if (listFind(closedList, adjacentSquare) != null) continue;
                    LocationRef loc = listFind(openList, adjacentSquare);
                    // If we haven't seen this square before then create the LocationRef
                    if (loc == null)
                    {
                        loc = new LocationRef {location = adjacentSquare, distanceFromStart = g};
                        loc.distanceToDestination = EstimateDistanceToDestination(loc.location, end);
                        loc.totalPath = loc.distanceFromStart + loc.distanceToDestination;
                        loc.Parent = current;

                        openList.AddFirst(loc);
                    }
                    else
                    {
                        // If we have seen this square before but haven't searched it
                        // then check whether we found a faster path to reach the square
                        if (g + loc.distanceToDestination < loc.totalPath)
                        {
                            loc.distanceFromStart = g;
                            loc.totalPath = loc.distanceFromStart + loc.distanceToDestination;
                            loc.Parent = current;
                        }
                    }
                }
            }

            var result = current;
            // Trace back to the first step, so you know where to move towards
            // Since we just produced a huge map of distances and values and shit there's a lot of
            // optimization and caching we could do before returning, but instead we're just throwing it all away
            while (result?.Parent != null && !isEqual(result.Parent, start))
            {
                result = result.Parent;
            }

            return result.location;
        }

        private static bool isEqual(LocationRef loc, int2 simple)
        {
            if (loc == null)
            {
                return false;
            }

            return loc.location.x == simple.x && loc.location.y == simple.y;
        }

        private static LocationRef listFind(LinkedList<LocationRef> list, int2 search)
        {
            foreach (var square in list)
            {
                if (isEqual(square, search)) return square;
            }

            return null;
        }

        private static LocationRef listFind(List<LocationRef> list, int2 search)
        {
            foreach (var square in list)
            {
                if (isEqual(square, search)) return square;
            }

            return null;
        }

        private static int EstimateDistanceToDestination(int2 start, int2 destination)
        {
            return math.abs(start.x - destination.x) + math.abs(start.y - destination.y);
        }

        private static LocationRef minList(LinkedList<LocationRef> list)
        {
            LocationRef found = list.First.Value;
            int lowest = found.totalPath;
            foreach (LocationRef item in list)
            {
                if (item.totalPath < lowest)
                {
                    lowest = item.totalPath;
                    found = item;
                }
            }

            return found;
        }


        private static List<int2> getAdjacentSquares(int2 start)
        {
            throw new NotImplementedException();
        }
    }
}