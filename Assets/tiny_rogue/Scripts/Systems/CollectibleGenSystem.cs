using System.Collections.Generic;
using game;
using Unity.Entities;

public class CollectibleGenSystem : ComponentSystem
{
    List<CollectibleEntry> collectiblesList;
    bool _isLoaded;
    
    protected override void OnCreate()
    {

    }
    
    public void GetRandomCollectible(EntityCommandBuffer ecb, Entity entity, CanBePickedUp c, HealthBonus hb )
    {
        if (collectiblesList.Count <= 0)
            return;
        
        var colEntry = collectiblesList[RandomRogue.Next(0, collectiblesList.Count)];
        c.appearance.sprite = GlobalGraphicsSettings.ascii ? colEntry.spriteAscii : colEntry.spriteGraphics;
        c.description = colEntry.description;
        c.name = colEntry.name;

        ecb.SetComponent(entity, c);
        
        if (colEntry.healthBonus != 0)
        {
            hb.healthAdded = colEntry.healthBonus;
            
            //TODO: fix this
            //entityManager.SetComponentData(entity, hb);
            ecb.SetComponent(entity, hb);

        }


    }

    protected override void OnUpdate()
    {
        if (_isLoaded) 
            return;
        
        
        collectiblesList = new List<CollectibleEntry>();
        
        Entities.WithAll<CollectibleLookup>().ForEach((lookup) =>
        {
            DynamicBuffer<CollectibleEntry>  collectibles = EntityManager.GetBuffer<CollectibleEntry>(lookup);
            
        
            if (collectibles.Length == 0)
                return;

            for (int i= 0; i < collectibles.Length; i++)
            {
                collectiblesList.Add(collectibles[i]);
            }

            _isLoaded = true;
        });

        
    }
}
