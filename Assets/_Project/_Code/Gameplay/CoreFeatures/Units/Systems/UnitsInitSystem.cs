using _Project._Code.Core.Contracts;
using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using _Project._Code.Gameplay.CoreFeatures.Units.Components;
using _Project._Code.Gameplay.CoreFeatures.Units.Factory;
using _Project._Code.Infrastructure.EcsContext;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Systems
{
    public sealed class UnitsInitSystem : IInit
    {
        private readonly IEcsContext _ecsContext;
        
        public UnitsInitSystem(
            IEcsContext ecsContext)
        {
            _ecsContext = ecsContext;
        }
        
        public void Init()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            var x = 0f;
            var z = 0f;
            for (int k = 0; k < 50; k++)
            {
                for (int i = 0; i < 100; i++)
                {
                    var request = ecb.CreateEntity();
                    ecb.AddComponent(request, new UnitFabricateRequest {
                        UnitId = UnitId.Footman,
                        Position = new float3(x, 0f, z),
                        Count = 1,
                        Team = 0
                    });
                    x += 1f;
                }
                z += 1f;
                x = 0f;
            }
            
            x = 0f;
            z = 57f;
            for (int k = 0; k < 50; k++)
            {
                for (int i = 0; i < 100; i++)
                {
                    var request = ecb.CreateEntity();
                    ecb.AddComponent(request, new UnitFabricateRequest {
                        UnitId = UnitId.Orc,
                        Position = new float3(x, 0f, z),
                        Count = 1,
                        Team = 1
                    });
                    
                    x += 1f;
                }
                z += 1f;
                x = 0f;
            }
        
            /*
            _unitFactory.Create(UnitId.Footman, new float3(0f, 0f, 0f), 0, ecb);
            _unitFactory.Create(UnitId.Footman, new float3(1f, 0f, 1f), 0, ecb);
            _unitFactory.Create(UnitId.Orc, new float3(25f, 0f, 25f), 1, ecb);
            _unitFactory.Create(UnitId.Orc, new float3(25f, 0f, 27f), 1, ecb);
            */
            ecb.Playback(_ecsContext.EntityManager);
        }
    }
}