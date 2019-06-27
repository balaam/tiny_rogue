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
    [UpdateAfter(typeof(FogOfWarSystem))]
    [UpdateInGroup(typeof(DisplaySystemGroup))]
    public class GameViewSystem : ComponentSystem
    {
        
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
        
        static Color GetColorForDoor(Tile tile)
        {
            Color color = TinyRogueConstants.DefaultColor;
            
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
        
        static float GetColorForMobileEntity(Tile tile)
        {
            return tile.IsSeen ? 1f : 0f;
        }

        protected override void OnUpdate()
        {
            var tileSprite = Sprite2DRenderer.Default;
            tileSprite.color = TinyRogueConstants.DefaultColor;

            // Set all floor tiles
            Entities.WithAll<Sprite2DRenderer, Floor>().ForEach((Entity e, ref Tile tile, ref Floor floor) =>
            {
                tileSprite.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('.', floor.TileOffset)];
                tileSprite.color.a = GetAlphaForStaticTile(tile);
                PostUpdateCommands.SetComponent(e, tileSprite);
            });

            // Default all block tiles to a wall
            
            Entities.WithAll<Sprite2DRenderer, Wall>().ForEach((Entity e, ref Tile tile, ref Wall wall) =>
            {
                tileSprite.sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('#', wall.TileOffset)];
                tileSprite.color.a = GetAlphaForStaticTile(tile);
                PostUpdateCommands.SetComponent(e, tileSprite);
            });
            
            // Set all door tiles// horizontal // vertical
            // Set all closed door tiles // closed horizontal // closed vertical
            var doorSprite = Sprite2DRenderer.Default;
            doorSprite.color = TinyRogueConstants.DefaultColor;
            Entities.WithAll<Sprite2DRenderer, Door>().ForEach((Entity e, ref Door door, ref WorldCoord coord) =>
            {
                if (door.Opened)
                    doorSprite.sprite = door.Horizontal ? SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('\\')] : SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('/')];
                else
                    doorSprite.sprite = door.Horizontal ? SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('_')]: SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics('|')];
                
                // Check the tile, regardless of what entity we're looking at; this will tell objects if their tile is visible or not
                var tileIndex = View.XYToIndex(new int2(coord.x, coord.y), GameStateSystem.GameView.Width);
                var tileEntity = GameStateSystem.GameView.ViewTiles[tileIndex];
                var tile = EntityManager.GetComponentData<Tile>(tileEntity);
                // Setting alpha on a door onto of a tile only works for ascii. For graphics, dim the colours instead.
                if (GlobalGraphicsSettings.ascii)
                {
                    doorSprite.color.a = GetAlphaForStaticTile(tile);
                }
                else
                {
                    doorSprite.color = GetColorForDoor(tile);
                }

                PostUpdateCommands.SetComponent(e, doorSprite);
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