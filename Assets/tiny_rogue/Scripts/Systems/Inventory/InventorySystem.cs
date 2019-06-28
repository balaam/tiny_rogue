using System.Collections.Generic;
using Unity.Collections;
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
        NativeList<InventoryItem> inventoryItems;
        LogSystem logSystem;
        bool collectItemsPressed;
        int2 playerPosition;
        

        protected override void OnCreate()
        {
            inventoryItems = new NativeList<InventoryItem>(16, Allocator.Persistent);
            logSystem = EntityManager.World.GetOrCreateSystem<LogSystem>();
            collectItemsPressed = false;
        }

        protected override void OnDestroy()
        {
           inventoryItems.Dispose();
        }

        protected override void OnUpdate()
        {
            if (collectItemsPressed)
            {
                Entities.WithAll<Collectible>().ForEach(
                    (Entity item, ref WorldCoord itemCoord, ref CanBePickedUp pickable) =>
                    {
                        if (playerPosition.x == itemCoord.x && playerPosition.y == itemCoord.y)
                        {
                            logSystem.AddLog($"You picked up a {pickable.name}");
                            AddItem(pickable);
                            
                            UseItem(pickable);

                            PostUpdateCommands.DestroyEntity(item);
                        }
                    });
                
                collectItemsPressed = false;
            }
            
        }

        public void RenderInventoryItems(NativeList<Sprite2DRenderer> spriteRenderers)
        {
            int loopLength = math.min(spriteRenderers.Length, inventoryItems.Length);
            for (int i = 0; i < loopLength; i++)
            {
                var renderer = spriteRenderers[i]; 
                renderer.sprite = inventoryItems[i].appearance.sprite;
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
            playerPosition = new int2(playerCoord.x, playerCoord.y);
            collectItemsPressed = true;

        }

        void AddItem(CanBePickedUp pickable)
        {
            inventoryItems.Add(new InventoryItem(){name = pickable.name, description = pickable.description, appearance = pickable.appearance});
        }

        void UseItem(CanBePickedUp pickable)
        {

            Entities.WithAll<Player>().ForEach((Entity player, ref WorldCoord coord, ref HealthPoints hp, 
                ref ArmorClass ac, ref AttackStat atak) =>
            {

                if (pickable.healthBonus != 0)
                {
                    hp.now += pickable.healthBonus;
                }

                if (pickable.armorBonus != 0)
                {
                    ac.AC += pickable.armorBonus;
                }

                if (pickable.attackBonus != 0)
                {
                    atak.range.x += pickable.attackBonus;
                    atak.range.y += pickable.attackBonus;
                }
            });
        }
    }
}
