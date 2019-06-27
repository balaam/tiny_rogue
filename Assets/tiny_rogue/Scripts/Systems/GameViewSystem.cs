using System;
using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Mathematics;
using Unity.Tiny.Input;
using UnityEngine;
using Color = Unity.Tiny.Core2D.Color;
using KeyCode = Unity.Tiny.Input.KeyCode;
using Random = Unity.Mathematics.Random;
#if !UNITY_WEBGL
using InputSystem = Unity.Tiny.GLFW.GLFWInputSystem;
#else
    using InputSystem =  Unity.Tiny.HTML.HTMLInputSystem;
#endif

namespace game
{
    // GameState drives the other systems.
    [UpdateAfter(typeof(TurnSystemGroup))]
    public class GameViewSystem : ComponentSystem
    {
        public static bool UpdateViewNeeded = false;
        
        static float GetAlphaForStaticTile(Tile tile)
        {
           float color = TinyRogueConstants.DefaultColor.a;
            
            if (!tile.IsSeen && tile.HasBeenRevealed)
            {
                color /= 2f;
            }
            else if (!tile.IsSeen)
            {
                color = 0f;
            }

            return color;
        }
        
        static float GetColorForMobileEntity(Tile tile)
        {
            return tile.IsSeen ? 1f : 0f;
        }

        protected override void OnUpdate()
        {
            // Don't do anything until we have sprites
            if (!SpriteSystem.Loaded || !UpdateViewNeeded)
                return;
            
            var tileSprite = Sprite2DRenderer.Default;

            // Set all floor tiles
            tileSprite.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('.')];
            Entities.WithAll<Sprite2DRenderer, Floor>().ForEach((Entity e, ref Tile tile) =>
            {
                tileSprite.color.a = GetAlphaForStaticTile(tile);
                PostUpdateCommands.SetComponent(e, tileSprite);
            });

            // Default all block tiles to a wall
            tileSprite.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('#')];
            Entities.WithAll<Sprite2DRenderer, Wall>().ForEach((Entity e, ref Tile tile) =>
            {
                tileSprite.color.a = GetAlphaForStaticTile(tile);
                PostUpdateCommands.SetComponent(e, tileSprite);
            });
            
            
            var horizontalDoorOpen = Sprite2DRenderer.Default;
            horizontalDoorOpen.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('\\')]; 

            var verticalDoorOpen = Sprite2DRenderer.Default;
            verticalDoorOpen.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('/')];
            
            var horizontalDoorClosed = Sprite2DRenderer.Default;
            horizontalDoorClosed.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('_')];
            
            var verticalDoorClosed = Sprite2DRenderer.Default;
            verticalDoorClosed.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('|')];
            
            // Set all door tiles// horizontal // vertical
            // Set all closed door tiles // closed horizontal // closed vertical
            Entities.WithAll<Sprite2DRenderer, Door>().ForEach((Entity e, ref Door door, ref WorldCoord coord) =>
            {
                Sprite2DRenderer spriteRenderer;
                if (door.Opened)
                    spriteRenderer = door.Horizontal ? horizontalDoorOpen : verticalDoorOpen;
                else
                    spriteRenderer = door.Horizontal ? horizontalDoorClosed : verticalDoorClosed;
                
                // Check the tile, regardless of what entity we're looking at; this will tell objects if their tile is visible or not
                var tileIndex = View.XYToIndex(new int2(coord.x, coord.y), GameStateSystem.GameView.Width);
                var tileEntity = GameStateSystem.GameView.ViewTiles[tileIndex];
                var tile = EntityManager.GetComponentData<Tile>(tileEntity);
                spriteRenderer.color.a = GetAlphaForStaticTile(tile);
                
                PostUpdateCommands.SetComponent(e, spriteRenderer);
            });
            
            // All remaining static entities can be updated but need to keep their colour
            // This reads from *before* the above changes, so shouldn't be used on the same entities
            Entities.WithNone<Mobile, Tile, Door>().ForEach(
                (Entity e, ref Sprite2DRenderer renderer, ref WorldCoord coord) =>
                {
                    var spriteRenderer = renderer;

                    // Check the tile, regardless of what entity we're looking at; this will tell objects if their tile is visible or not
                    var tileIndex = View.XYToIndex(new int2(coord.x, coord.y), GameStateSystem.GameView.Width);
                    var tileEntity = GameStateSystem.GameView.ViewTiles[tileIndex];
                    var tile = EntityManager.GetComponentData<Tile>(tileEntity);
                    spriteRenderer.color.a = GetAlphaForStaticTile(tile);

                    PostUpdateCommands.SetComponent(e, spriteRenderer);
                });
            
            // All mobile entities should be either visible or not
            Entities.WithAll<Mobile>().ForEach(
                (Entity e, ref Sprite2DRenderer renderer, ref WorldCoord coord) =>
                {
                    var spriteRenderer = renderer;

                    // Check the tile, regardless of what entity we're looking at; this will tell objects if their tile is visible or not
                    var tileIndex = View.XYToIndex(new int2(coord.x, coord.y), GameStateSystem.GameView.Width);
                    var tileEntity = GameStateSystem.GameView.ViewTiles[tileIndex];
                    var tile = EntityManager.GetComponentData<Tile>(tileEntity);
                    spriteRenderer.color.a = GetColorForMobileEntity(tile);

                    PostUpdateCommands.SetComponent(e, spriteRenderer);
                });
        }
    }
}