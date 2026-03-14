using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Components
{
    public struct MovementStats : IComponentData //TODO Вынести в blob
    {
        public float Speed;
        public float RotationSpeed;
    }
}