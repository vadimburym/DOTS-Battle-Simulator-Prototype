using Unity.Entities;
using Unity.Mathematics;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Components
{
    public struct MoveCommandRequest : IComponentData
    {
        public float3 Destination;
    }
}