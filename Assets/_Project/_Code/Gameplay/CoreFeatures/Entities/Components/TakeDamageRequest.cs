using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Components
{
    public struct TakeDamageRequest : IComponentData
    {
        public ushort Damage;
        public Entity Source;
    }
}