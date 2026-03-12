using Unity.Entities;
using Unity.Mathematics;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Components
{
    public struct MoveCommandSingleton : IComponentData
    {
        public float3 Destination;
        public byte IsIssued;
        //NativeArray<Entity>
    }
}