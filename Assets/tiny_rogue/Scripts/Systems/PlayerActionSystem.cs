using Unity.Entities;
using Unity.Tiny.Core2D;
using UnityEngine;

namespace game
{
    public enum Action
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Wait,
        Interact,
        None
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
        }

        public void Wait()
        {
            var log = EntityManager.World.GetExistingSystem<LogSystem>();
            log.AddLog("You wait a turn.");
            tms.NeedToTickTurn = true;
        }

        public void TryMove(Entity e, WorldCoord c, bool alternateMoveAction, EntityCommandBuffer commandBuffer)
        {
            if (!alternateMoveAction)
            {
                Entities.WithNone<BlockMovement>().WithAll<Tile>().ForEach(
                    (ref WorldCoord tileCoord, ref Translation tileTrans) =>
                    {
                        // This location the player wants to move has nothing blocking them, so update their position.
                        if (tileCoord.x == c.x && tileCoord.y == c.y)
                        {
                            EntityManager.SetComponentData(e, tileCoord);
                            EntityManager.SetComponentData(e, tileTrans);
                            tms.NeedToTickTurn = true;
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
                }
            });
        }
    }
}