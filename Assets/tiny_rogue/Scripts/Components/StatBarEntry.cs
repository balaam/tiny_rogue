using Unity.Entities;

namespace game
{
    public enum StatType
    {
        Hp,
        Gold,
        Level,
        Floor,
        Xp
    }

    public struct StatBarEntry : IComponentData
    {
        public StatType Stat;
    }
}
