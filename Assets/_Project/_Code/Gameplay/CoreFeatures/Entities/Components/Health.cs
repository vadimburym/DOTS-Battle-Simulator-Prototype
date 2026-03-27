using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Components
{
    public struct Health : IComponentData
    {
        public ushort Max;
        public ushort Current;
    }
}