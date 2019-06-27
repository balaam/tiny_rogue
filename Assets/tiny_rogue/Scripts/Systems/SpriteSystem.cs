using System;
using Unity.Entities;
using Unity.Tiny.Core2D;

namespace game
{
    public enum SpriteEnum
    {
        EmptyTile = 0,
        Spikes,
        Crown,
        Stairway,
        Collectible,
        Gold,
        OpenVerticalDoor,
        ClosedVerticalDoor,
        OpenHorizontalDoor,
        ClosedHorizontalDoor,
        Wall,
        Floor,
        Floor1,
        Floor2,
        Floor3,
        HealthPotion
    }
    /// <summary>
    /// Reads in the ASCII sprites and stores then in an array indexable by the chars decimal value. i.e. A is 65
    /// </summary>
    public class SpriteSystem : ComponentSystem
    {
        public static Entity[] IndexSprites = new Entity[256];
        static bool _loaded = false;

        public static bool Loaded
        {
            get { return _loaded; }
        }

        public static char ConvertToGraphics(char c)
        {
            char result = c;

            if (GlobalGraphicsSettings.ascii)
                return result;

            switch (c)
            {
                case ' ':
                    // TODO: need to figure out empty/none tile
                    result = (char) SpriteEnum.EmptyTile;
                    break;
                case '^':
                    result = (char) SpriteEnum.Spikes;
                    break;
                case (char)127:
                    result = (char) SpriteEnum.Crown;
                    break;
                case 'Z':
                    result = (char) SpriteEnum.Stairway;
                    break;
                case 'S':
                    result = (char) SpriteEnum.Collectible;
                    break;
                case (char)236:
                    result = (char) SpriteEnum.Gold;
                    break;
                case (char)235:
                    result = (char)SpriteEnum.HealthPotion;
                    break;
                case '/':
                    result = (char) SpriteEnum.OpenVerticalDoor;
                    break;
                case '|':
                    result = (char) SpriteEnum.ClosedVerticalDoor;
                    break;
                case '\\':
                    result = (char) SpriteEnum.OpenHorizontalDoor;
                    break;
                case '_':
                    result = (char) SpriteEnum.ClosedHorizontalDoor;
                    break;
                case '#':
                    result = (char) SpriteEnum.Wall;
                    break;
                case '.':
                    result = (char) SpriteEnum.Floor;
                    break;
            }

            return result;
        }

        protected override void OnUpdate()
        {
            if (SpriteSystem._loaded)
                return;

            // Get the graphics settings
            Entities.ForEach((ref GraphicsSettings gs) => {
                GlobalGraphicsSettings.ascii = gs.ascii;
                GlobalGraphicsSettings.TileSize = gs.TileSize;
            });

            Entities.WithAll<SpriteLookUp>().ForEach((Entity entity) =>
            {
                DynamicBuffer<SpriteAtlas> atlas = EntityManager.GetBuffer<SpriteAtlas>(entity);

                for (int i = 0; i < atlas.Length; i++)
                    IndexSprites[i] = atlas[i].sprite;

                SpriteSystem._loaded = true;
            });
        }
    }
}
