using Unity.Entities;
using Unity.Tiny.Core2D;
using UnityEngine;

namespace game
{
    public struct CanBePickedUp : IComponentData
    {
        public NativeString64 name;
        public NativeString64 description;
        public Sprite2DRenderer appearance;
    }
}
