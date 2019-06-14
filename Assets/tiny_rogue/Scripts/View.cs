using System;
using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Mathematics;

namespace game
{
	// The view is the viewport shown on screen that's made up of ASCII characters.
	// For this game we're probably going to have the view pretty much the same as the map.
	// But for a more advanced game the view just displays one part of larger multiscreen map.
	
	// The data is filled in at run time.
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
	    
	    public void Blit(EntityManager em, int x, int y, string s)
	    {
		    int writeToX = x;
		    foreach (char c in s)
		    {
			    Blit(em, new int2(writeToX, y), c);
			    writeToX++;
		    }
	    }

		// Get this working then see if you can remove entity manager, or cache one
	    public void Blit(EntityManager em, int2 xy, int c)
	    {
		    Entity e = ViewTiles[XYToIndex(xy, Width)];
		    Sprite2DRenderer s = em.GetComponentData<Sprite2DRenderer>(e);
		    s.sprite = SpriteSystem.AsciiToSprite[c];
		    em.SetComponentData(e, s);
	    }	    
    }
}