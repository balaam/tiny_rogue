using System;
using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Mathematics;
using UnityEngine;
using Color = Unity.Tiny.Core2D.Color;

namespace game
{
	/// <summary>
	/// The view describes what the player can see. It's the viewport into the world.
	/// The viewport is made up of a grid of tiles that are ASCII characters.
	/// In this game the world is the same size as the viewport.
	/// In a more advanced game the view would display only one part of larger multiscreen map.
	/// </summary>
    public class View
    {
	    // A width * height list of tiles that make up the view
	    public Entity[] ViewTiles;

	    // Width and Height in tiles (not pixels or something else).
	    public int Width;
	    public int Height;

	    public static int2 IndexToXY(int i, int width)
	    {
		    int2 xy = new int2(i % width, i / width);
		    return xy;
	    }

	    public static int XYToIndex(int2 xy, int width)
	    {
		    return (xy.y * width) + xy.x;
	    }

	    public void Blit(EntityCommandBuffer ecb, int2 xy, string s)
	    {
		    Blit(ecb, xy, s, TinyRogueConstants.DefaultColor);
	    }
	    public void Blit(EntityCommandBuffer ecb, int2 xy, string s, Color color)
	    {
		    int writeToX = xy.x;
		    foreach (char c in s)
		    {
			    Blit(ecb, new int2(writeToX, xy.y), c, color);
			    writeToX++;
		    }
	    }

	    public void Blit(EntityCommandBuffer ecb, int2 xy, int c)
	    {
		    Blit(ecb, xy, c, TinyRogueConstants.DefaultColor);
	    }
	    public void Blit(EntityCommandBuffer ecb, int2 xy, int c, Color color)
	    {
			if (!GlobalGraphicsSettings.ascii) return;
		    Entity e = ViewTiles[XYToIndex(xy, Width)];
		    
		    ecb.SetComponent(e, new Sprite2DRenderer
		    {
			    sprite = SpriteSystem.IndexSprites[SpriteSystem.ConvertToGraphics((char)c)],
			    color = color
		    });
	    }

	    public void ClearLine(EntityCommandBuffer ecb, int line, char clearChar)
	    {
		    ClearLine(ecb, line, clearChar, TinyRogueConstants.DefaultColor);
	    }
	    public void ClearLine(EntityCommandBuffer ecb, int line, char clearChar, Color color)
	    {
		    for (int i = 0; i < Width; i++)
			    Blit(ecb, new int2(i, line), clearChar, color);
	    }

	    /// <summary>
	    /// Given an integer coordinate in view space translate it to unity world space position
	    /// </summary>
	    /// <param name="coord">A coordinate in the viewport</param>
	    /// <returns>A position in world space units at the position of the view coord.</returns>
	    public float3 ViewCoordToWorldPos(int2 coord)
	    {
		    var startX = -(math.floor(Width / 2) * GlobalGraphicsSettings.TileSize.x);
		    var startY = math.floor(Height / 2) * GlobalGraphicsSettings.TileSize.y;

		    var pos = new float3(
			    startX + (coord.x * GlobalGraphicsSettings.TileSize.x),
			    startY - (coord.y * GlobalGraphicsSettings.TileSize.y), 0);
		    return pos;
	    }
    }
}