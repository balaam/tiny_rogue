using System;
using Unity.Entities;
using Unity.Tiny.Core2D;

namespace game
{
    public class SpriteSystem : ComponentSystem
    {
        // Probably need wrapping up into some better names classes
        public static Entity[] AsciiToSprite = new Entity[256];
        static bool _loaded = false;

        public static bool Loaded
        {
            get { return _loaded; }
        }

        protected override void OnUpdate()
        {
            if (SpriteSystem.Loaded)
                return;

            Entities.ForEach((Entity entity, ref SpriteLookUp lookUp) =>
            {
                // This bit is generated, not sure of a better way to handle this at the moment.  
                // See Utils/gen_char_map.rb

                DynamicBuffer<SpriteAtlas> o = EntityManager.GetBuffer<SpriteAtlas>(entity);

                for (int i = 0; i < o.Length; i++)
                    AsciiToSprite[i] = o[i].sprite;

                SpriteSystem._loaded = true;
            });

        }
    }
}
