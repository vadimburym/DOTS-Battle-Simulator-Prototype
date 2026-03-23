using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures.AI._Root
{
    [InternalBufferCapacity(0)]
    public struct LeafStateWriteRequest : IComponentData
    {
        public Entity Entity;
        public ushort Index;
        public Entity Value;
    }
}