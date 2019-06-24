using Unity.Entities;

namespace game
{
    public struct HealthPoints : IComponentData 
    {
    	public int now;
    	public int max;
    }
}
