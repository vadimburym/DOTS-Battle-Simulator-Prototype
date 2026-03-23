using _Project._Code.Core.Contracts;
using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using _Project._Code.Gameplay.CoreFeatures.Units.Factory;
using _Project._Code.Infrastructure.EcsContext;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Systems
{
    public sealed class UnitsInitSystem : IInit
    {
        private readonly IUnitFactory _unitFactory;
        private readonly IEcsContext _ecsContext;
        
        public UnitsInitSystem(
            IUnitFactory unitFactory,
            IEcsContext ecsContext)
        {
            _unitFactory = unitFactory;
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
                    var entity = _unitFactory.Create(UnitId.Footman, new float3(x, 0f, z), 0, ecb);
                    
                    var request = ecb.CreateEntity();
                    ecb.AddComponent(request, new MoveCommandRequest {
                        Destination = new float3(x, 0f, z)
                    });
                    ecb.AddBuffer<MoveCommandTarget>(request);
                    ecb.AppendToBuffer(request, new MoveCommandTarget {
                        Value = entity
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
                    var entity = _unitFactory.Create(UnitId.Orc, new float3(x, 0f, z), 1, ecb);
                    
                    var request = ecb.CreateEntity();
                    ecb.AddComponent(request, new MoveCommandRequest {
                        Destination = new float3(x, 0f, z)
                    });
                    ecb.AddBuffer<MoveCommandTarget>(request);
                    ecb.AppendToBuffer(request, new MoveCommandTarget {
                        Value = entity
                    });
                    
                    x += 1f;
                }
                z += 1f;
                x = 0f;
            }
            /*
            _unitFactory.Create(UnitId.Footman, new float3(0f, 0f, 0f), 0, ecb);
            _unitFactory.Create(UnitId.Footman, new float3(1f, 0f, 1f), 0, ecb);
            _unitFactory.Create(UnitId.Footman, new float3(5f, 0f, 5f), 1, ecb);
            _unitFactory.Create(UnitId.Footman, new float3(6f, 0f, 6f), 1, ecb);
            */
            ecb.Playback(_ecsContext.EntityManager);
        }
    }
}