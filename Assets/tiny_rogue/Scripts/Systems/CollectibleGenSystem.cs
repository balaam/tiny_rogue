using System.Collections.Generic;
using game;
using Unity.Collections;
using Unity.Entities;

public class CollectibleGenSystem : ComponentSystem
{
    NativeList<CollectibleEntry> collectiblesList;
    bool _isLoaded;
    
    protected override void OnCreate()
    {
        collectiblesList = new NativeList<CollectibleEntry>(16,Allocator.Persistent);
        
    }

    protected override void OnDestroy()
    {
        collectiblesList.Dispose();
    }

    public void GetRandomCollectible( ref CanBePickedUp c, ref HealthBonus hb )
    {
        if (collectiblesList.Length <= 0)
            return;
        
        var colEntry = collectiblesList[RandomRogue.Next(0, collectiblesList.Length)];
        c.appearance.sprite = GlobalGraphicsSettings.ascii ? colEntry.spriteAscii : colEntry.spriteGraphics;
        c.description = colEntry.description;
        c.name = colEntry.name;
        c.healthBonus = colEntry.healthBonus;
        c.armorBonus = colEntry.armorBonus;
        c.attackBonus = colEntry.attackBonus;

        
        if (colEntry.healthBonus != 0)
        {
            hb.healthAdded = colEntry.healthBonus;
        }
    }

    protected override void OnUpdate()
    {
        if (_isLoaded) 
            return;
        
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
