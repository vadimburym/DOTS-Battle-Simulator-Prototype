using _Project._Code.Core.Contracts;
using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.Units.Factory;
using Unity.Mathematics;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Systems
{
    public sealed class UnitsInitSystem : IInit
    {
        private readonly IUnitFactory _unitFactory;
        
        public UnitsInitSystem(
            IUnitFactory unitFactory)
        {
            _unitFactory = unitFactory;
        }
        
        public void Init()
        {
            
            var x = 0f;
            var z = 0f;
            for (int k = 0; k < 100; k++)
            {
                for (int i = 0; i < 100; i++)
                {
                    _unitFactory.Create(UnitId.Footman, new float3(x, 0f, z), (byte)(i % 2));
                    x += 1f;
                }
                z += 1f;
                x = 0f;
            }
            
            //_unitFactory.Create(UnitId.Footman, new float3(0f, 0f, 0f), 0);
            //_unitFactory.Create(UnitId.Footman, new float3(1f, 0f, 1f), 0);
        }
    }
}