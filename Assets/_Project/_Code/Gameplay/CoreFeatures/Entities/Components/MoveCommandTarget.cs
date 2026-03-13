using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Components
{
    [InternalBufferCapacity(32)]
    public struct MoveCommandTarget : IBufferElementData
    {
        public Entity Value;
    }
}