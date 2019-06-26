using Unity.Entities;

namespace game
{
    
    public struct Creature : IComponentData
    {
        public int id;
    }

    public struct tag_Attackable : IComponentData 
    {
    }

    public struct tag_Corpse : IComponentData
    {
        
    }
}