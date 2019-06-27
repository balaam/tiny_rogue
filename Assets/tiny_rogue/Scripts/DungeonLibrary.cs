using Unity.Entities;

namespace game
{
    public struct CreatureSpawnParams
    {
        public ECreatureId[] Creatures;
        public int SpawnMin;
        public int SpawnMax;
        
    }
    
    public struct DungeonGenParams
    {
        public CreatureSpawnParams[] CreatureSpawns;
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
                }
            },
        };

        public static DungeonGenParams GetDungeonParams(int level, bool isFinalLevel)
        {
            return DungeonSetups[0];
        }
    }
}
