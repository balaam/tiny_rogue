using Unity.Entities;
using Unity.Tiny.Core2D;
using UnityEngine;

namespace game
{
    public enum Action
    {
        None = 0,
        MoveUp = 1,
        MoveDown = 2,
        MoveLeft = 3,
        MoveRight = 4,
        Wait = 5,
        Interact = 6,
        Attack = 7,
        Move = 8
    }

    public class PlayerActionSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
        }

        private GameStateSystem gss;
        private TurnManagementSystem tms;

        protected override void OnCreate()
        {
            gss = EntityManager.World.GetOrCreateSystem<GameStateSystem>();
            tms = EntityManager.World.GetOrCreateSystem<TurnManagementSystem>();
            base.OnCreate();
        }

        public void Interact(WorldCoord c)
        {
            Entities.WithAll<Stairway>().ForEach((ref WorldCoord stairCoord, ref Translation stairTrans) =>
            {
                if (c.x == stairCoord.x && c.y == stairCoord.y)
                    gss.MoveToNextLevel(PostUpdateCommands);
            });

            var inventorySystem = EntityManager.World.GetExistingSystem<InventorySystem>();
            inventorySystem.CollectItemsAt(c);
        }

        public void Wait()
        {
            var log = EntityManager.World.GetExistingSystem<LogSystem>();
            log.AddLog("You wait a turn.");
            tms.NeedToTickTurn = true;
        }

        public bool TryMove(Entity e, WorldCoord c, bool alternateMoveAction, EntityCommandBuffer commandBuffer)
        {
            bool moved = false;
            if (!alternateMoveAction)
            {
                Entities.WithNone<BlockMovement>().WithAll<Tile>().ForEach(
                    (ref WorldCoord tileCoord, ref Translation tileTrans) =>
                    {
                        // This location the player wants to move has nothing blocking them, so update their position.
                        if (tileCoord.x == c.x && tileCoord.y == c.y)
                        {
                            EntityManager.SetComponentData(e, tileCoord);
                            // Graphical will animate the sprite to the new position.
                            if (GlobalGraphicsSettings.ascii)
                            {
                                EntityManager.SetComponentData(e, tileTrans);
                            }
                            else
                            {
                                var player = EntityManager.GetComponentData<Player>(e);
                                player.Initial = EntityManager.GetComponentData<Translation>(e).Value;
                                player.Destination = tileTrans.Value;
                                EntityManager.SetComponentData(e, player);
                            }
                            tms.NeedToTickTurn = true;
                            moved = true;
                        }
                    });
            }

            Entities.WithAll<Door>().ForEach((Entity doorEntity, ref WorldCoord tileCoord, ref Sprite2DRenderer renderer, ref Door door) =>
            {
                if (tileCoord.x == c.x && tileCoord.y == c.y)
                {
                    var log = EntityManager.World.GetExistingSystem<LogSystem>();
                    if (!door.Opened)
                    {
                        log.AddLog("You opened a door.");
                        door.Opened = true;
                        commandBuffer.RemoveComponent(doorEntity, typeof(BlockMovement));
                        renderer.sprite = SpriteSystem.IndexSprites['\\'];
                    }
                    else if (alternateMoveAction)
                    {
                        log.AddLog("You closed a door.");
                        door.Opened = false;
                        commandBuffer.AddComponent(doorEntity, new BlockMovement());
                        renderer.sprite = SpriteSystem.IndexSprites['|'];
                    }

                    tms.NeedToTickTurn = true;
                    var inventorySystem = EntityManager.World.GetExistingSystem<InventorySystem>();
                    inventorySystem.LogItemsAt(c);
                }
            });
            
            return moved;
        }
    }
}
