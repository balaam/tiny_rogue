        using System;
        using Unity.Entities;

namespace game
{
    public class ItemUseSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<OnUsed>().ForEach(item =>
            {
                Entities.WithAll<Player>().ForEach(player =>
                {
                    if (EntityManager.HasComponent(item, typeof(HealthBonus)))
                    {
                        var hp = EntityManager.GetComponentData<HealthPoints>(player);
                        var healthBonus = EntityManager.GetComponentData<HealthBonus>(item);
                        hp.now += healthBonus.healthAdded;
                    }
                    
                    
                });

                PostUpdateCommands.DestroyEntity(item);
            });
        }
    }

}
