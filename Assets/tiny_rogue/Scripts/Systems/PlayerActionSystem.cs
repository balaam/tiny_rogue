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

            var inventorySystem = EntityManager.World.GetExistingSystem<InventorySystem>();
            inventorySystem.CollectItemsAt(c);
        }

        public void Wait()
        {
            var log = EntityManager.World.GetExistingSystem<LogSystem>();
            log.AddLog("You wait a turn.");
            tms.NeedToTickTurn = true;
        }

        public void TryMove(Entity e, WorldCoord c)
        {
            Entities.WithNone<BlockMovement>().WithAll<Tile>().ForEach((ref WorldCoord tileCoord, ref Translation tileTrans) =>
            {
                // This location the player wants to move has nothing blocking them, so update their position.
                if (tileCoord.x == c.x && tileCoord.y == c.y)
                {
                    EntityManager.SetComponentData(e, tileCoord);
                    EntityManager.SetComponentData(e, tileTrans);                        
                    tms.NeedToTickTurn = true;
                    var inventorySystem = EntityManager.World.GetExistingSystem<InventorySystem>();
                    inventorySystem.LogItemsAt(c);
               }
            });
        }
    }
}
