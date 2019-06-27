using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace game
{


    [UpdateInGroup(typeof(TurnSystemGroup))]
    public class HealthItemsSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            LogSystem log = EntityManager.World.GetExistingSystem<LogSystem>();
            GameStateSystem gss = EntityManager.World.GetExistingSystem<GameStateSystem>();
            TurnManagementSystem tms = EntityManager.World.GetExistingSystem<TurnManagementSystem>();
            View view = gss.View;

            Entity[] healingItems = new Entity[view.Width * view.Height];
            Entities.WithAll<HealItem>().ForEach((Entity e, ref WorldCoord coord) =>
            {
                int i = View.XYToIndex(new int2(coord.x, coord.y), view.Width);
                healingItems[i] = e;
            });

            Entities.WithAll<Player>().ForEach((Entity e, ref WorldCoord coord, ref HealthPoints hp) =>
            {
                int i = View.XYToIndex(new int2(coord.x, coord.y), view.Width);
                if (EntityManager.HasComponent(healingItems[i], typeof(HealItem)))
                {
                    HealItem heal = EntityManager.GetComponentData<HealItem>(healingItems[i]);

                    if (hp.now + heal.HealAmount > hp.max)
                        heal.HealAmount = (hp.max - hp.now);

                    hp.now += heal.HealAmount;

                    if (heal.HealAmount > 0)
                        log.AddLog("Healed for " + heal.HealAmount.ToString() + " points.");
                    else if (heal.HealAmount == 0)
                        log.AddLog("Healed for 0 points.  ...that's disappointing.");
                    else if (heal.HealAmount < 0)
                    {
                        string dmgLog = "The Kobolds have poisoned the potion!!  " + (-1 * heal.HealAmount).ToString() + " damage taken!";
                        log.AddLog(dmgLog);
                        gss.LastPlayerHurtLog = dmgLog; 
                    }
                    PostUpdateCommands.DestroyEntity(healingItems[i]);
                }
            });
        }
    }
}
