using System;
using Unity.Entities;
using Unity.Tiny;
using Unity.Tiny.Core2D;


namespace game
{
    public struct CollectibleEntry : IBufferElementData
    {
        [EntityWithComponents(new Type[] {typeof (Sprite2D)})]
        public Entity spriteGraphics;

        [EntityWithComponents(new Type[] {typeof (Sprite2D)})]
        public Entity spriteAscii;

        public NativeString64 name;

        public NativeString64 description;

        public int healthBonus;

        public int attackBonus;

        public int armorBonus;

    }
}
