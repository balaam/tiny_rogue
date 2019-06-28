
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
            /* Boss Level */
            new DungeonGenParams 
            {
                CreatureSpawns = new CreatureSpawnParams[]
                {
                    new CreatureSpawnParams
                    {
                        Creatures = new ECreatureId[] { ECreatureId.Kobold },
                        SpawnMin = 10,
                        SpawnMax = 16
                    },
                    
                    new CreatureSpawnParams
                    {
                        Creatures = new ECreatureId[] { ECreatureId.KoboldChampion },
                        SpawnMin = 2,
                        SpawnMax = 2
                    },
                    
                    new CreatureSpawnParams
                    {
                        Creatures = new ECreatureId[] { ECreatureId.KoboldKing },
                        SpawnMin = 1,
                        SpawnMax = 1
                    },
                },
                PotionSpawns = new PotionSpawnParams[]
                {
                    new PotionSpawnParams { SpawnMin = 3, SpawnMax = 5, ValueMin = 3, ValueMax = 6},
                    new PotionSpawnParams { SpawnMin = 0, SpawnMax = 1, ValueMin = -10, ValueMax = -6}
                },
                CollectibleSpawns = new CollectibleSpawnParams[] {},
                GoldSpawn = new GoldSpawnParams { SpawnMin = 20, SpawnMax = 40},
                TrapSpawn = new TrapSpawnParams { SpawnMin = 0, SpawnMax = 0}
            },
            /* Level 1 */
            new DungeonGenParams 
            {
                CreatureSpawns = new CreatureSpawnParams[]
                {
                    new CreatureSpawnParams
                    {
                        Creatures = new ECreatureId[] { ECreatureId.Kobold },
                        SpawnMin = 6,
                        SpawnMax = 8
                    }
                },
                PotionSpawns = new PotionSpawnParams[]
                {
                    new PotionSpawnParams { SpawnMin = 1, SpawnMax = 3, ValueMin =2, ValueMax = 5},
                },
                CollectibleSpawns = new CollectibleSpawnParams[] {},
                GoldSpawn = new GoldSpawnParams { SpawnMin = 0, SpawnMax = 5},
                TrapSpawn = new TrapSpawnParams { SpawnMin = 1, SpawnMax = 1}
            },
            /* Level 2 */
            new DungeonGenParams 
            {
                CreatureSpawns = new CreatureSpawnParams[]
                {
                    new CreatureSpawnParams
                    {
                        Creatures = new ECreatureId[] { ECreatureId.Fireskull, ECreatureId.Kobold },
                        SpawnMin = 6,
                        SpawnMax = 10
                    }
                },
                PotionSpawns = new PotionSpawnParams[]
                {
                    new PotionSpawnParams { SpawnMin = 1, SpawnMax = 3, ValueMin = 1, ValueMax = 6},
                    new PotionSpawnParams { SpawnMin = 0, SpawnMax = 2, ValueMin = -4, ValueMax = -2}
                },
                CollectibleSpawns = new CollectibleSpawnParams[] {},
                GoldSpawn = new GoldSpawnParams { SpawnMin = 2, SpawnMax = 10},
                TrapSpawn = new TrapSpawnParams { SpawnMin = 1, SpawnMax = 3}
            },
        };

        public static DungeonGenParams GetDungeonParams(int level, bool isFinalLevel)
        {
            if (isFinalLevel)
            {
                return DungeonSetups[0];
            }

            switch (level)
            {
                case 1:
                case 2:
                    return DungeonSetups[level];
                default:
                    return DungeonSetups[2];
                
            }
        }
    }
}
