# Tiny Rogue

A simple roguelike made to explore Unity Tiny and ECS style programming.

## Requirements

*UnityEditor:* Latest 2019.2.x
I'm using 0b5 (e3a10156d6de) which is an internal release.

*com.unity.tiny* I'm using the latest from the DOTs repo on the `release/tinymode` branch.
ca9a2da5ce6ded73fb0cb97d7733afe10b61a769

1. Make a directory like `hw19`.
2. Checkout dots into `hw19/dots`.
3. Checkout this repo into `hw19/rogue` (the dir name doesn't matter)

It's more complicated than it otherwise would be because we're using the very latest version of Tiny so we can use C#. It doesn't use the "cloud" package manager but instead references one on disk. 

## Running the Project

If you open the project and press play you should get a player with word "Hello" written in the center in an ASCII font.

## 2d Graphics and Project Setup Details

Character size: 
    9 x 16 pixels
    0.09 x 0.16 world space units
Resolution: 
    80 x 25 characters
    720 x 400 pixels
Camera half vertical height:
    2 world units

### Graphic Setup Details

The character set included in this project is from an IBM PC and contains ASCII and a number of extended characters.

The glyphs in the texture are width:9 x height:16 in pixels.
Console terminals can display width:80 x height:25 in characters.
Giving a pixel resolution of 720 x 400 pixels.

Unity has a `Pixels Per Unity` setting that maps the size of a single pixel to world units. By default it's 100. This means 100 pixels would fit in a single unit in Unity. Some internal systems consider a world unit to be 1 meter but for this game it can just be considered to be a arbitary unit.

The size of a single ASCII character in Unity units can worked out by dividing it's pixel size by the PPU.
```
width:	9  / 100 = 0.09
height: 16 / 100 = 0.16 
```

If there's a character at 0,0 and you want a tile to sit flush next to it then you set that tiles position to 0.09, 0.

