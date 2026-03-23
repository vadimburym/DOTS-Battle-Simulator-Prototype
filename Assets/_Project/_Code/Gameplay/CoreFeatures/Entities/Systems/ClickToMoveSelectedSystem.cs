using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using _Project._Code.Infrastructure;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using VContainer;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Systems
{
    [DisableAutoCreation]
    public partial class ClickToMoveSelectedSystem : SystemBase
    {
        [Inject] private readonly IInputService _inputService;
        
        protected override void OnUpdate()
        {
            if (!_inputService.IsSecondActionDown)
                return;
            if (!_inputService.TryGetMouseToWorldPosition(out var worldPosition))
                return;
            
            var selectedQuery = SystemAPI.QueryBuilder()
                .WithAll<SelectedTag, LocalTransform, TargetPosition>()
                .Build();

            int count = selectedQuery.CalculateEntityCount();
            if (count == 0)
                return;

            var selectedEntities = selectedQuery.ToEntityArray(Allocator.Temp);
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged);
            var requestEntity = ecb.CreateEntity();

            ecb.AddComponent(requestEntity, new MoveCommandRequest {
                Destination = worldPosition
            });

            var buffer = ecb.AddBuffer<MoveCommandTarget>(requestEntity);
            for (int i = 0; i < selectedEntities.Length; i++)
            {
                buffer.Add(new MoveCommandTarget {
                    Value = selectedEntities[i]
                });
            }

            selectedEntities.Dispose();
        }
    }
}