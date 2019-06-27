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
        
        static Unity.Tiny.Core2D.Color GetColorForTile(Tile tile)
        {
            Unity.Tiny.Core2D.Color color = TinyRogueConstants.DefaultColor;
            
            if (!tile.IsSeen && tile.HasBeenRevealed)
            {
                color.r /= 2f;
                color.g /= 2f;
                color.b /= 2f;
            }
            else if (!tile.IsSeen)
            {
                color.a = 0f;
            }

            return color;
        }
        
        private static Unity.Tiny.Core2D.Color GetColorForObject(Tile tile)
        {
            var color = TinyRogueConstants.DefaultColor;

            if (!tile.IsSeen)
            {
                color.a = 0f;
            }

            return color;
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
                tileSprite.color = GetColorForTile(tile);
                PostUpdateCommands.SetComponent(e, tileSprite);
            });

            // Default all block tiles to a wall
            tileSprite.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('#')];
            Entities.WithAll<Sprite2DRenderer, Wall>().ForEach((Entity e, ref Tile tile) =>
            {
                tileSprite.color = GetColorForTile(tile);
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
                spriteRenderer.color = GetColorForTile(tile);
                
                PostUpdateCommands.SetComponent(e, spriteRenderer);
            });
            
            // This reads from *before* the above changes, so shouldn't be used on the same entities
            Entities.WithNone<Player, Tile, Door>().ForEach(
                (Entity e, ref Sprite2DRenderer renderer, ref WorldCoord coord) =>
                {
                    var spriteRenderer = renderer;

                    // Check the tile, regardless of what entity we're looking at; this will tell objects if their tile is visible or not
                    var tileIndex = View.XYToIndex(new int2(coord.x, coord.y), GameStateSystem.GameView.Width);
                    var tileEntity = GameStateSystem.GameView.ViewTiles[tileIndex];
                    var tile = EntityManager.GetComponentData<Tile>(tileEntity);

                    spriteRenderer.color.a = tile.IsSeen ? 1 : 0;

                    PostUpdateCommands.SetComponent(e, spriteRenderer);
                });
        }
    }
}