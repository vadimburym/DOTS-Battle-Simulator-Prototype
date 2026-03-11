using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using _Project._Code.Infrastructure;
using Unity.Entities;
using VContainer;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Systems
{
    [DisableAutoCreation]
    public partial class MoveCommandSystem : SystemBase
    {
        [Inject] private readonly IInputService _inputService;
        
        protected override void OnUpdate()
        {
            if (_inputService.IsSecondActionDown)
            {
                if (_inputService.TryGetMouseToWorldPosition(out var worldPosition))
                {
                    foreach (var (targetPosition, _) in 
                             SystemAPI.Query<RefRW<TargetPosition>, RefRO<Selected>>())
                    {
                        targetPosition.ValueRW.Value = worldPosition;
                    }
                }
            }
        }
    }
}