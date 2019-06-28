using Unity.Entities;

namespace game
{
    public struct CreatureSpawnParams
    {
        public ECreatureId[] Creatures;
        public int SpawnMin;
        public int SpawnMax;
    }

    public struct PotionSpawnParams
    {
        public int SpawnMin;
        public int SpawnMax;
        public int ValueMin;
        public int ValueMax;
    }

    public struct CollectibleSpawnParams
    {
        public int SpawnMin;
        public int SpawnMax;
    }

    public struct GoldSpawnParams
    {
        public int SpawnMin;
        public int SpawnMax;
    }

    public struct TrapSpawnParams
    {
        public int SpawnMin;
        public int SpawnMax;
    }
    
    public struct DungeonGenParams
    {
        public CreatureSpawnParams[] CreatureSpawns;
        public PotionSpawnParams[] PotionSpawns;
        public CollectibleSpawnParams[] CollectibleSpawns;
        public GoldSpawnParams GoldSpawn;
        public TrapSpawnParams TrapSpawn;
    }
    
    public class DungeonLibrary
    {
        private static readonly DungeonGenParams[] DungeonSetups = new DungeonGenParams[]
        {
            new DungeonGenParams 
            {
                CreatureSpawns = new CreatureSpawnParams[]
                {
                    new CreatureSpawnParams
                    {
                        Creatures = new ECreatureId[] { ECreatureId.Fireskull, ECreatureId.Kobold },
                        SpawnMin = 4,
                        SpawnMax = 10
                    }
                },
                PotionSpawns = new PotionSpawnParams[]
                {
                    new PotionSpawnParams { SpawnMin = 1, SpawnMax = 3, ValueMin = 1, ValueMax = 6},
                    new PotionSpawnParams { SpawnMin = 0, SpawnMax = 2, ValueMin = -4, ValueMax = -2}
                },
                CollectibleSpawns = new CollectibleSpawnParams[] {},
                GoldSpawn = new GoldSpawnParams { SpawnMin = 0, SpawnMax = 10},
                TrapSpawn = new TrapSpawnParams { SpawnMin = 2, SpawnMax = 3}
            },
        };

        public static DungeonGenParams GetDungeonParams(int level, bool isFinalLevel)
        {
            return DungeonSetups[0];
        }
    }
}
