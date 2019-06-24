using Unity.Entities;

namespace game
{
    public abstract class TurnSystem : ComponentSystem
    {
        public TurnSystem()
        {
            TurnManager.RegisterSystem(this);
        }

        public abstract void OnTurn(uint turnNumber);
    }
}
