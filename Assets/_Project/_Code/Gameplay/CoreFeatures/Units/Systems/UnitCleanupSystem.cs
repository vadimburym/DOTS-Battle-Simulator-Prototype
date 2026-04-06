using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using _Project._Code.Gameplay.CoreFeatures.Entities.Systems;
using _Project._Code.Gameplay.CoreFeatures.Units.Service;
using Unity.Burst;
using Unity.Entities;
using VContainer;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Systems
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(EntityCleanupSystem))]
    [BurstCompile]
    public partial class UnitCleanupSystem : SystemBase
    {
        [Inject] private IUnitCounterService _unitCounterService;
        
        [BurstCompile]
        protected override void OnUpdate()
        {
            var query = SystemAPI.QueryBuilder().WithAll<CleanupTag>().Build();
            var count = query.CalculateEntityCount();
            if (count == 0)
                return;
            _unitCounterService.Decrease(count);
        }
    }
}