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
            
            Entities.WithAll<InventoryComponent>().ForEach((Entity creature, ref WorldCoord coord) =>
            {
                int2 creaturePos = new int2(coord.x, coord.y);

                Entities.WithAll<Collectible>().ForEach((Entity item, ref WorldCoord itemCoord, ref CanBePickedUp pickable) =>
                {
                    if (creaturePos.x == itemCoord.x && creaturePos.y == itemCoord.y)
                    {
                        if(EntityManager.HasComponent(creature, typeof(Player)))
                        {
                            var log = EntityManager.World.GetExistingSystem<LogSystem>();
                            log.AddLog("You found a " + pickable.name);
                            
                            var input = EntityManager.World.GetExistingSystem<InputSystem>();

                            if (input.GetKeyDown(KeyCode.Z))
                            {
                                //Add item you're on into the inventory
                                AddItem(pickable.name, pickable.description, pickable.appearance);
                                PostUpdateCommands.DestroyEntity(item);
                            }
                        }
                    }
                });
            });


        }

        public void AddItem(NativeString64 inName, NativeString64 inDesc, Sprite2DRenderer spr)
        {
            var Items = EntityManager.GetBuffer<InventoryItem>(inventoryEntity);
            Items.Add(new InventoryItem(){name = inName, description = inDesc, appearance = spr});
        }
    }
}
