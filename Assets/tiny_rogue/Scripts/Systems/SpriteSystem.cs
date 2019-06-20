using System;
using Unity.Entities;
using Unity.Tiny.Core2D;

namespace game
{
    /// <summary>
    /// Reads in the ASCII sprites and stores then in an array indexable by the chars decimal value. i.e. A is 65
    /// </summary>
    public class SpriteSystem : ComponentSystem
    {
        public static Entity[] AsciiToSprite = new Entity[256];
        static bool _loaded = false;

        public static bool Loaded
        {
            get { return _loaded; }
        }

        protected override void OnUpdate()
        {
            if (SpriteSystem._loaded)
                return;

            Entities.WithAll<SpriteLookUp>().ForEach((Entity entity) =>
            {
                DynamicBuffer<SpriteAtlas> atlas = EntityManager.GetBuffer<SpriteAtlas>(entity);

                for (int i = 0; i < atlas.Length; i++)
                    AsciiToSprite[i] = atlas[i].sprite;

                SpriteSystem._loaded = true;
            });
        }
    }
}
