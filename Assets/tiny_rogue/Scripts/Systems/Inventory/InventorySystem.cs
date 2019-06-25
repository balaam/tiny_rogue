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

        protected override void OnCreate()
        {
            inventoryEntity = EntityManager.CreateEntity();
            EntityManager.AddBuffer<InventoryItem>(inventoryEntity);
        }

        protected override void OnDestroy()
        {
            EntityManager.DestroyEntity(inventoryEntity);
        }

        protected override void OnUpdate()
        {
            
        }

        public void LogItemsAt(WorldCoord playerCoord)
        {
            int2 playerPos = new int2(playerCoord.x, playerCoord.y);

            Entities.WithAll<Collectible>().ForEach(
                (Entity item, ref WorldCoord itemCoord, ref CanBePickedUp pickable) =>
                {
                    if (playerPos.x == itemCoord.x && playerPos.y == itemCoord.y)
                    {
                        var log = EntityManager.World.GetExistingSystem<LogSystem>();
                        log.AddLog("You found a " + pickable.name);
                    }
                });
        }

        public void CollectItemsAt(WorldCoord playerCoord)
        {
            int2 playerPos = new int2(playerCoord.x, playerCoord.y);
            var log = EntityManager.World.GetExistingSystem<LogSystem>();
            var pis = EntityManager.World.GetExistingSystem<PlayerInputSystem>();

            Entities.WithAll<Collectible>().ForEach(
                (Entity item, ref WorldCoord itemCoord, ref CanBePickedUp pickable) =>
                {
                    if (playerPos.x == itemCoord.x && playerPos.y == itemCoord.y)
                    {
                        log.AddLog("You picked up a " + pickable.name);
                        AddItem(pickable.name, pickable.description, pickable.appearance);
                        
                        pis.PostUpdateCommands.DestroyEntity(item);
                    }
                });
        }

        public void AddItem(NativeString64 inName, NativeString64 inDesc, Sprite2DRenderer spr)
        {
            var Items = EntityManager.GetBuffer<InventoryItem>(inventoryEntity);
            Items.Add(new InventoryItem(){name = inName, description = inDesc, appearance = spr});
        }
    }
}
