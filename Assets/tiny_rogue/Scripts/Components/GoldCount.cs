using Unity.Entities;

namespace game
{
    public struct GoldCount : IComponentData 
    {
        //This is the gold in the player's pocket
    	public int count;
    }
}
