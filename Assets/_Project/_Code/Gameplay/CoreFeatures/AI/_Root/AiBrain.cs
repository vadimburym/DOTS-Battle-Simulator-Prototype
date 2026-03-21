using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures.AI._Root
{
    public struct AiBrain : IComponentData
    {
        public byte BlobId;
        public float UpdateTime;
    }
}