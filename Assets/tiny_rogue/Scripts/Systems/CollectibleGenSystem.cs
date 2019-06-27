using game;
using Unity.Entities;

public class CollectibleGenSystem : ComponentSystem
{
    public void GetRandomCollectible(EntityCommandBuffer ecb, Entity entity, CanBePickedUp c, HealthBonus hb )
    {
        CollectibleEntry colEntry = new CollectibleEntry();
        
        Entities.WithAll<CollectibleLookup>().ForEach((lookup) =>
        {
            DynamicBuffer<CollectibleEntry>  collectibles = EntityManager.GetBuffer<CollectibleEntry>(lookup);
            
        
            if (collectibles.Length == 0)
                return;
        
            colEntry = collectibles[RandomRogue.Next(0, collectibles.Length)];
            c.appearance.sprite = GlobalGraphicsSettings.ascii ? colEntry.spriteAscii : colEntry.spriteGraphics;
            c.description = colEntry.description;
            c.name = colEntry.name;

            ecb.SetComponent(entity, c);
        
            if (colEntry.healthBonus != 0)
            {
                hb.healthAdded = colEntry.healthBonus;
            
                //TODO: fix this
                //ecb.SetComponent(entity, hb);
            }


        });
        

    }

    protected override void OnUpdate()
    {
        
    }
}
