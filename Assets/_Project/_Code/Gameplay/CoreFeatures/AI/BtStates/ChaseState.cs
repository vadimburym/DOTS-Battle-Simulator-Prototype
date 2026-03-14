using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Components
{
    public struct ChaseState : IComponentData
    {
        public Entity Owner;
        public Entity Target;
        public float UpdateInterval;
        public float UpdateTimer;
    }
}