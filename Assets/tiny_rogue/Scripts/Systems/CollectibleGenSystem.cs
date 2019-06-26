using game;
using Unity.Entities;

public class CollectibleGenSystem : ComponentSystem
{
    DynamicBuffer<CollectibleEntry> collectibles;
    
    protected override void OnCreate()
    {
        Init();
    }

    void Init()
    {
        Entities.WithAll<SpriteLookUp>().ForEach((entity) =>
        {
            collectibles = EntityManager.GetBuffer<CollectibleEntry>(entity);
        });
    }

    public void GetRandomCollectible(ref Entity item, ref CanBePickedUp c )
    {
        var collectible = collectibles[RandomRogue.Next(0, collectibles.Length)];

        c.appearance.sprite = GlobalGraphicsSettings.ascii ? collectible.spriteAscii : collectible.spriteGraphics;
        c.description = collectible.description;
        c.name = collectible.name;

        if (collectible.healthBonus != 0)
        {
            var healthBonus = new HealthBonus();
            healthBonus.healthAdded = collectible.healthBonus;
            EntityManager.SetComponentData(item,healthBonus);
        }
        
    }

    protected override void OnUpdate()
    {
    }
}
