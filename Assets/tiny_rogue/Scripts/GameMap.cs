using System;
using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Mathematics;

namespace game
{
	/// <summary>
	/// The view describes what the player can see. It's the viewport into the world.
	/// The viewport is made up of a grid of tiles that are ASCII characters.
	/// In this game the world is the same size as the viewport.
	/// In a more advanced game the view would display only one part of larger multiscreen map.
	/// </summary>
    public class GameMap
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
	    
	    public void Blit(EntityManager em, int2 xy, int c)
	    {
		    Entity e = ViewTiles[XYToIndex(xy, Width)];
		    Sprite2DRenderer s = em.GetComponentData<Sprite2DRenderer>(e);
		    s.sprite = SpriteSystem.IndexSprites[c];
		    em.SetComponentData(e, s);
	    }

	    /// <summary>
	    /// Given an integer coordinate in view space translate it to unity world space position
	    /// </summary>
	    /// <param name="coord">A coordinate in the viewport</param>
	    /// <returns>A position in world space units at the position of the view coord.</returns>
	    public float3 ViewCoordToWorldPos(int2 coord)
	    {
		    var startX = -(math.floor(Width/2) * TinyRogueConstants.TileWidth);
		    var startY = math.floor(Height / 2) * TinyRogueConstants.TileHeight;
		    
		    var pos = new float3(
			    startX + (coord.x * TinyRogueConstants.TileWidth), 
			    startY - (coord.y * TinyRogueConstants.TileHeight), 0);
		    return pos;
	    }

    }
}