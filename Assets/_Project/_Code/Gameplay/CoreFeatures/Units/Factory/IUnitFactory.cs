using _Project._Code.Core.Keys;
using Unity.Entities;
using Unity.Mathematics;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Factory
{
    public interface IUnitFactory
    {
        Entity Create(UnitId unitId, float3 position, byte team, EntityCommandBuffer ecb);
    }
}