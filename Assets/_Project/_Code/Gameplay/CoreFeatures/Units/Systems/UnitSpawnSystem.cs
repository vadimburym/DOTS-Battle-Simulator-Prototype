using _Project._Code.Core.Keys;
using _Project._Code.Gameplay.CoreFeatures.Entities.Behaviours;
using _Project._Code.Gameplay.CoreFeatures.Units.Components;
using _Project._Code.Gameplay.CoreFeatures.Units.Service;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.EcsContext;
using Unity.Burst;
using Unity.Entities;
using VContainer;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Systems
{
    [DisableAutoCreation]
    [BurstCompile]
    [UpdateInGroup(typeof(LocalSystemsGroup), OrderFirst = true)]
    public partial class UnitSpawnSystem : SystemBase
    {
        [Inject] private IUnitSpawnService _unitSpawnService;
        [Inject] private IInputService _inputService;
        [Inject] private ISelectionAreaProvider _selectionAreaProvider;
        
        [BurstCompile]
        protected override void OnUpdate()
        {
            if (_inputService.IsMainActionDown &&
                _unitSpawnService.IsSpawnMode &&
                _selectionAreaProvider.InBounds(_inputService.MousePosition) &&
                _inputService.TryGetMouseToWorldPosition(out var position))
            {
                var spawnData = _unitSpawnService.UnitSpawnData;
                var request = EntityManager.CreateEntity();
                EntityManager.AddComponentData(request, new UnitFabricateRequest {
                    Position = position,
                    UnitId = spawnData.UnitId,
                    Count = spawnData.Count,
                    Team = spawnData.UnitId == UnitId.Footman ? (byte)0 : (byte)1, //TEMPORARY
                });
            }
        }
    }
}