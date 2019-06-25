using Unity.Entities;
using Unity.Tiny.Core2D;
using UnityEngine;

public struct InventoryItem : IBufferElementData
{

    public NativeString64 name;
    public NativeString64 description;
    public Sprite2DRenderer appearance;

}
