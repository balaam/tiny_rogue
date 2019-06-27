using Unity.Entities;
using Unity.Mathematics;

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
            
            GameStateSystem gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
            View view = gss.View;

            bool[] blockedPosition = new bool[view.Height * view.Width];
            Entities.WithAll<BlockMovement>().ForEach((Entity e, ref WorldCoord coord) =>
            {
                int i = View.XYToIndex(new int2(coord.x, coord.y), view.Width);
                blockedPosition[i] = true;
            });

            // Monsters stop Patrolling and start actively following the Player if they can spot the Player
            Entities.WithAll<PatrollingState, Sight>().ForEach(
                (Entity e, ref WorldCoord coord, ref Sight sight) =>
                {
                    int2 pos = new int2(coord.x, coord.y);
                    float totalDistance = math.sqrt(math.pow(math.distance(playerPos.x, pos.x), 2) +
                                                    math.pow(math.distance(playerPos.y, pos.y), 2));
                    
                    if (totalDistance <= viewDepth && !SightBlocked(playerPos, pos, view, blockedPosition))
                    {
                        PostUpdateCommands.RemoveComponent(e, typeof(PatrollingState));
                    }
                });

            // Determine whether tile is visible to Player
            Entities.ForEach((Entity e, ref WorldCoord coord) =>
            {
                int2 pos = new int2(coord.x, coord.y);
                int tileIndex = View.XYToIndex(pos, view.Width);
                Tile tile = EntityManager.GetComponentData<Tile>(view.ViewTiles[tileIndex]);

                float totalDistance = math.sqrt(math.pow(math.distance(playerPos.x, pos.x), 2) +
                                                math.pow(math.distance(playerPos.y, pos.y), 2));
                
                if (totalDistance <= viewDepth && !SightBlocked(playerPos, pos, view, blockedPosition))
                {
                    tile.IsSeen = true;
                    tile.HasBeenRevealed = true;
                }
                else
                    tile.IsSeen = false;

                EntityManager.SetComponentData(view.ViewTiles[tileIndex], tile);
            });
        }

        private bool SightBlocked(int2 start, int2 end, View view, bool[] blockedPositions)
        {
            float x = (end - start).x;
            float y = (end - start).y;

            float xDirection = math.sign(x);
            float yDirection = math.sign(y);

            int2 currentTile = start;

            while (!currentTile.Equals(end))
            {
                if (currentTile.x != end.x)
                    currentTile.x += (int)xDirection;

                if (currentTile.y != end.y)
                    currentTile.y += (int)yDirection;

                if (blockedPositions[View.XYToIndex(currentTile, view.Width)] && !currentTile.Equals(end))
                    return true;
            }

            return false;
        }
    }
}