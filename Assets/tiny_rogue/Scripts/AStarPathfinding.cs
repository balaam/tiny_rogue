using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Tiny.Core2D;

namespace game
{
    // 4-directional A* Algorithm
    // TODO optimize with RSR or JPS
    // TODO currently this roguelike is 4-directional only, this algorithm will need minor rewrites if we switch to 8-directional
    public class AStarPathfinding
    {
        private class Path
        {
            // Tile coordinates
            public int2 location;
            public int totalPath;
            public int distanceFromStart;
            public int distanceToDestination; // this is an estimated value
            public Path Parent;

            // Based on a fully populated A* search path, this identifies which tile to move into from the current location
            public Path stepFrom(int2 currentLocation)
            {
                var result = this;
                // Trace back to the first step, so you know where to move towards
                while (result?.Parent != null && !isEqual(result.Parent, currentLocation))
                {
                    result = result.Parent;
                }

                return result;
            }

            // Convert a fully-defined Path into a struct Path which is easily usable by DOTS code
            public SavedPath toSavedPath()
            {
                var result = new SavedPath();
                var step = this;
                var length = 0;
                while (step.Parent != null)
                {
                    length++;
                    step = step.Parent;
                }

                var steps = new NativeArray<int2>(length, Allocator.Persistent);
                step = this;
                for(var i = length - 1; i >= 0; i--)
                {
                    steps[i] = step.location;
                    step = step.Parent;
                }
                result.pathSteps = steps;
                
                // We don't actually use the first step since it should be the current position
                result.currentIdx = 1;
                return result;
            }
        }
        
        public int2 stepAlong(SavedPath path, int2 currentLocation)
        {
            if (path.currentIdx >= path.pathSteps.Length) return currentLocation;
            var result = path.pathSteps[path.currentIdx];
            path.currentIdx++;
            return result;
        }

        // The main method you should use to identify where a given monster will step next
        public static int2 getNextStep(int2 start, int2 end)
        {
            return _getPath(start, end).stepFrom(start).location;
        }

        public static SavedPath getPath(int2 start, int2 end)
        {
            return _getPath(start, end).toSavedPath();
        }

        // The A* algorithm returning the entire Path from Start to End. Usually getNextStep will be better
        // though but this is exposed for caching purposes e.g. with patrolling monsters.
        private static Path _getPath(int2 start, int2 end)
        {
            Path current = null;
            var startLoc = new Path {location = start};
            var closedList = new List<Path>();
            var openList = new List<Path>();
            var travelDistance = 0;

            openList.Add(startLoc);
            while (openList.Count > 0)
            {
                current = minList(openList);
                openList.Remove(current);
                closedList.Add(current);
                // Did we just find the end square? If so then we're finished!
                if (current.location.x == end.x && current.location.y == end.y) break;
                var adjacentSquares = getWalkableAdjacentSquares(current.location);
                travelDistance++;
                foreach (var adjacentSquare in adjacentSquares)
                {
                    // If we've already thoroughly searched this square then do nothing
                    if (listFind(closedList, adjacentSquare) != null) continue;
                    Path loc = listFind(openList, adjacentSquare);
                    // If we haven't seen this square before then create the LocationRef
                    if (loc == null)
                    {
                        loc = new Path {location = adjacentSquare, distanceFromStart = travelDistance};
                        loc.distanceToDestination = EstimateDistanceToDestination(loc.location, end);
                        loc.totalPath = loc.distanceFromStart + loc.distanceToDestination;
                        loc.Parent = current;

                        openList.Add(loc);
                    }
                    else
                    {
                        // If we have seen this square before but haven't searched it
                        // then check whether we found a faster path to reach the square
                        if (travelDistance + loc.distanceToDestination < loc.totalPath)
                        {
                            loc.distanceFromStart = travelDistance;
                            loc.totalPath = loc.distanceFromStart + loc.distanceToDestination;
                            loc.Parent = current;
                        }
                    }
                }
            }

            return current;
        }

        private static bool isEqual(Path loc, int2 simple)
        {
            if (loc == null)
            {
                return false;
            }

            return loc.location.x == simple.x && loc.location.y == simple.y;
        }

        private static Path listFind(List<Path> list, int2 search)
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

        private static Path minList(List<Path> list)
        {
            Path found = null;
            int lowest = Int32.MaxValue;

            // Reverse order iteration - most recently inserted items are the most likely candidates
            for (var i = list.Count - 1; i >= 0; i--)
            {
                Path item = list[i];
                if (item.totalPath < lowest)
                {
                    lowest = item.totalPath;
                    found = item;
                }
            }

            return found;
        }

        private static List<int2> getWalkableAdjacentSquares(int2 start)
        {
            //TODO proper implementation, currently all monsters are either ghosts or idiots since Walkability is not considered
            return new List<int2>()
            {
                new int2(start.x + 1, start.y),
                new int2(start.x - 1, start.y),
                new int2(start.x, start.y + 1),
                new int2(start.x, start.y - 1)
            };
        }
    }
}