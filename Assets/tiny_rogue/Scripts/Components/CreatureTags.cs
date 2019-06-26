using Unity.Entities;
using Unity.Mathematics;

namespace game
{
    
    public struct Creature : IComponentData
    {
        public int id;
    }

    public struct AttackStat : IComponentData
    {
        public int2 range;
    }

    public struct tag_Attackable : IComponentData 
    {
    }

    public struct tag_Corpse : IComponentData
    {
        
    }
}