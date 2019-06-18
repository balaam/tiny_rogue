# Tiny Rogue

A simple roguelike made to explore Unity Tiny and ECS style programming.
[Play it here](https://balaam.github.io/tiny_rogue/)

## Requirements

*UnityEditor:* Version 2019.2.x
I'm using 0b5 (e3a10156d6de).

(and it's using Unity Tiny 0.15.3-preview but this should get setup automatically for you via the Package Manager).

## Running the Project

If you open the project and press play you should get a player in a big room.

## Release

Set the project to build in Web (ASM JS) under release.
Do a build.
Delete the data in the `./docs` directory.
Copy the data from [Library/DotsRuntimeBuild/build/tiny_rogue/tiny_rogue-asmjs-release] to [./docs]
Rename `tiny_rogue.html` to `index.html`

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

### Contributors

_Add your name here to confirm you can push to this repository._

[@danielsc](https://github.com/balaam) <br/>
[@jimmy-jam](https://github.com/jimmy-jam) <br/>
[@chris-addison](https://github.com/chris-addison) <br />



