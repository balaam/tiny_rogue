using Unity.Entities;

namespace game
{

    [UpdateInGroup(typeof(TurnSystemGroup))]
    public abstract class TurnSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            var tms = EntityManager.World.GetOrCreateSystem<TurnManagementSystem>();
            tms.RegisterTurnSystem(this);
        }

    }
}
