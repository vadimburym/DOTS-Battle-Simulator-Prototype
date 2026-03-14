using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures
{
    public struct EyeSensor : IComponentData
    {
        public byte IsDetected;
        public Entity DetectedEntity;
        public float UpdateNearestTimer;
        public float ScanTimer;
    }
}