using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;

namespace game
{
    public class FogOfWarSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<Sprite2DRenderer>().ForEach((Entity e, ref Sprite2DRenderer renderer, ref WorldCoord coord) =>
            {
                View view = EntityManager.World.GetExistingSystem<GameStateSystem>().View;
                int tileIndex = View.XYToIndex(new int2(coord.x, coord.y), view.Width);
                Entity tileEntity = view.ViewTiles[tileIndex];
                Tile tile = EntityManager.GetComponentData<Tile>(tileEntity);

                if (tile.HasBeenRevealed)
                    renderer.color.a = TinyRogueConstants.DefaultColor.a;
            });
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
            //this is dumb, be a little smarter about how you calculate the tiles to check.
            for (float x = -1f; x < 1f; x += 0.1f)
            {
                for (float y = -1f; y < 1f; y += 0.1f)
                {
                    CheckDirection(startPosition, sightDepth, view, i => { return i + x; }, i => { return i + y; });
                }
            }
        }

        private void CheckDirection(int2 position, int checkDepth, View view, Func<float, float> xOp, Func<float, float> yOp)
        {
            float xFloatStore = position.x;
            float yFloatStore = position.y;

            for (int i = 0; i < checkDepth; i++)
            {
                //get entity
                int tileIndex = View.XYToIndex(new int2((int)xFloatStore, (int)yFloatStore), view.Width);
                Entity tileEntity = view.ViewTiles[tileIndex];
                Tile tile = EntityManager.GetComponentData<Tile>(tileEntity);

                tile.HasBeenRevealed = true;
                EntityManager.SetComponentData(tileEntity, tile);

                //return if blocks movement
                if (EntityManager.HasComponent(tileEntity, typeof(BlockMovement)))
                    return;

                xFloatStore = xOp(xFloatStore);
                yFloatStore = yOp(yFloatStore);
            }
        }
    }
}