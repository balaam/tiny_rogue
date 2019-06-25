using Unity.Entities;

namespace game
{

    [UpdateAfter(typeof(TurnManagementSystem))]
    [UpdateInGroup(typeof(TurnSystemGroup))]
    public abstract class TurnSystem : ComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            var tms = EntityManager.World.GetOrCreateSystem<TurnManagementSystem>();
            tms.RegisterTurnSystem(this);
        }

        protected override void OnDestroy()
        {
            var tms = EntityManager.World.GetExistingSystem<TurnManagementSystem>();
            tms.UnregisterTurnSystem(this);
            base.OnDestroy();
        }
    }
}
