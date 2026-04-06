using _Project._Code.Core.Keys;
using Unity.Entities;
using Unity.Mathematics;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Components
{
    public struct UnitFabricateRequest : IComponentData
    {
        public UnitId UnitId;
        public int Count;
        public float3 Position;
        public byte Team;
    }
}