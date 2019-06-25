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
        Interact = 6
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
                }
            });
        }
    }
}