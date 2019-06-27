using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;
using Unity.Tiny.Input;
using UnityEngine;
using KeyCode = Unity.Tiny.Input.KeyCode;

namespace game
{

    public class InventorySystem : ComponentSystem
    {
        Entity inventoryEntity;
        int2 lastPosition;
        LogSystem logSystem;
        

        protected override void OnCreate()
        {
            inventoryEntity = EntityManager.CreateEntity();
            EntityManager.AddBuffer<InventoryItem>(inventoryEntity);
            logSystem = EntityManager.World.GetOrCreateSystem<LogSystem>();
        }

        protected override void OnDestroy()
        {
            EntityManager.DestroyEntity(inventoryEntity);
        }

        protected override void OnUpdate()
        {
            
        }

        public void RenderInventoryItems(List<Sprite2DRenderer> spriteRenderers)
        {
            var Items = EntityManager.GetBuffer<InventoryItem>(inventoryEntity);
            int loopLength = math.min(spriteRenderers.Count, Items.Length);
            for (int i = 0; i < loopLength; i++)
            {
                var renderer = spriteRenderers[i]; 
                renderer.sprite = Items[i].appearance.sprite;
            }
           
        }

        public void LogItemsAt(WorldCoord playerCoord)
        {
            int2 playerPos = new int2(playerCoord.x, playerCoord.y);

            Entities.WithAll<Collectible>().ForEach(
                (Entity item, ref WorldCoord itemCoord, ref CanBePickedUp pickable) =>
                {
                    if (playerPos.x == itemCoord.x && playerPos.y == itemCoord.y)
                    {
                        logSystem.AddLog($"You found a {pickable.name}");
                    }
                });
        }

        public void CollectItemsAt(EntityCommandBuffer ecb, WorldCoord playerCoord)
        {
            int2 playerPos = new int2(playerCoord.x, playerCoord.y);

            Entities.WithAll<Collectible>().ForEach(
                (Entity item, ref WorldCoord itemCoord, ref CanBePickedUp pickable) =>
                {
                    if (playerPos.x == itemCoord.x && playerPos.y == itemCoord.y)
                    {
                        logSystem.AddLog($"You picked up a {pickable.name}");
                        AddItem(pickable);
                        
                        ecb.DestroyEntity(item);
                    }
                });
        }

        public void AddItem(CanBePickedUp pickable)
        {
            var Items = EntityManager.GetBuffer<InventoryItem>(inventoryEntity);
            Items.Add(new InventoryItem(){name = pickable.name, description = pickable.description, appearance = pickable.appearance});
        }
    }
}
