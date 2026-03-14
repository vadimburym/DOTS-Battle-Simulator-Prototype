using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Components
{
    public struct AttackStats : IComponentData //TODO Вынести в blob
    {
        public byte AttackRangeCells;
        public float AttackInterval;
        public ushort Damage;
    }
}