using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace _Project._Code.Gameplay.CoreFeatures
{
    [BurstCompile]
    [DisableAutoCreation]
    [UpdateAfter(typeof(GridOccupancySyncSystem))]
    public partial struct GridReservationCleanupSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GridRuntimeMapSingleton>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var mapsRw = SystemAPI.GetSingletonRW<GridRuntimeMapSingleton>();
            var reservedMap = mapsRw.ValueRW.ReservedMap;

            foreach (var (
                         body,
                         gridState,
                         reachedTag,
                         entity) in
                     SystemAPI.Query<
                         RefRO<UnitBody>,
                         RefRW<GridNavigationState>,
                         EnabledRefRW<GridReservationReached>>()
                         .WithEntityAccess())
            {
                if (!reachedTag.ValueRO)
                    continue;
                if (gridState.ValueRO.HasReservedCell == 0)
                {
                    reachedTag.ValueRW = false;
                    continue;
                }

                int footprintX = math.max(1, body.ValueRO.FootprintX);
                int footprintY = math.max(1, body.ValueRO.FootprintY);

                BattlefieldGridUtils.ReleaseArea(
                    reservedMap,
                    entity,
                    gridState.ValueRO.ReservedCell,
                    footprintX,
                    footprintY);

                gridState.ValueRW.HasReservedCell = 0;
                reachedTag.ValueRW = false;
            }
        }
    }
}