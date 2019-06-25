using game;
using Unity.Mathematics;
using Unity.Entities;

namespace game
{
    // The WinCondition detection should be finer and only trigger a specific number of events.
    public class WinConditionSystem : TurnSystem
    {
        protected override void OnUpdate() 
        {
            int2 playerPos = new int2(0,0);
                        
            // The player has just moved, have they won the game?
            Entities.WithAll<Crown>().ForEach((ref WorldCoord crownPos) =>
            {
                Entities.WithAll<Player>().ForEach((Entity player, ref WorldCoord coord) =>
                {
                    playerPos.x = coord.x;
                    playerPos.y = coord.y;
                });
                
                if (playerPos.x == crownPos.x && playerPos.y == crownPos.y)
                {
                    var gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
                    gss.MoveToGameWin(PostUpdateCommands);    
                }
            });
        }
    }
}
