using Unity.Entities;

namespace game
{
    public struct LastMove : IComponentData 
    {
        //This keeps track of if the player was just on the stairs or not
    	public bool wasOnStairs;
    }
}
