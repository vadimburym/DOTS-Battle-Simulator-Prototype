using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures
{
    public struct EyeSensorStats : IComponentData //TODO в Blob
    {
        public float DetectRadius;
        public float ChaseRadius;
        public float UpdateNearestInterval;
        public float ScanInterval;
    }
}