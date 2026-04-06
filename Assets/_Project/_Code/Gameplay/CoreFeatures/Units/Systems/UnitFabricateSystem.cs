using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using _Project._Code.Gameplay.CoreFeatures.Units.Components;
using _Project._Code.Gameplay.CoreFeatures.Units.Factory;
using _Project._Code.Gameplay.CoreFeatures.Units.Service;
using _Project._Code.Infrastructure.EcsContext;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using VContainer;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Systems
{
    [DisableAutoCreation]
    [BurstCompile]
    [UpdateInGroup(typeof(LocalSystemsGroup), OrderFirst = true)]
    public partial class UnitFabricateSystem : SystemBase
    {
        [Inject] private IUnitCounterService _unitCounterService;
        [Inject] private IUnitFactory _unitFactory;
        
        [BurstCompile]
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var count = 0;
            foreach (var (requestData, request) in 
                     SystemAPI.Query<RefRO<UnitFabricateRequest>>().WithEntityAccess())
            {
                ecb.DestroyEntity(request);
                
                var moveRequest = ecb.CreateEntity();
                ecb.AddComponent(moveRequest, new MoveCommandRequest {
                    Destination = requestData.ValueRO.Position
                });
                ecb.AddBuffer<MoveCommandTarget>(moveRequest);

                for (int i = 0; i < requestData.ValueRO.Count; i++)
                {
                    var unit = _unitFactory.Create(
                        requestData.ValueRO.UnitId,
                        requestData.ValueRO.Position,
                        requestData.ValueRO.Team,
                        ecb);
                    ecb.AppendToBuffer(moveRequest, new MoveCommandTarget {
                        Value = unit
                    });
                    count++;
                }
            }

            if (count == 0)
                return;
            _unitCounterService.Increase(count);
            ecb.Playback(EntityManager);
        }
    }
}