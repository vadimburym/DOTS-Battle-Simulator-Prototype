using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Components
{
    [InternalBufferCapacity(16)]
    public struct MoveCommandTarget : IBufferElementData
    {
        public Entity Value;
    }
}