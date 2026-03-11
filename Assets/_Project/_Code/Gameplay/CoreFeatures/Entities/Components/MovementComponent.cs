using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Components
{
    public struct MovementComponent : IComponentData
    {
        public float Speed;
        public float RotationSpeed;
    }
}