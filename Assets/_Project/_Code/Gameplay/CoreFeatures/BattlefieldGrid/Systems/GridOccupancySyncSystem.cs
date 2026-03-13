using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Project._Code.Gameplay.CoreFeatures
{
    [BurstCompile]
    [DisableAutoCreation]
    [UpdateAfter(typeof(GridRuntimeMapSystem))]
    public partial struct GridOccupancySyncSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BattlefieldGridSingleton>();
            state.RequireForUpdate<GridRuntimeMapSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var gridRef = SystemAPI.GetSingleton<BattlefieldGridSingleton>().Value;
            var job = new GridOccupancySyncJob {
                GridRef = gridRef
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct GridOccupancySyncJob : IJobEntity
        {
            public BlobAssetReference<BattlefieldGridBlob> GridRef;

            [BurstCompile]
            private void Execute(
                ref GridNavigationState gridState,
                EnabledRefRW<GridReservationReached> reservationReachedTag,
                in LocalTransform transform,
                in UnitBody body,
                in TargetPosition targetPosition)
            {
                ref var grid = ref GridRef.Value;

                int2 occupiedCell = BattlefieldGridUtils.WorldToCell(ref grid, transform.Position);

                gridState.OccupiedCell = occupiedCell;
                gridState.HasOccupiedCell = 1;

                reservationReachedTag.ValueRW = false;

                if (gridState.HasReservedCell == 0)
                    return;

                float3 toTarget = targetPosition.Position - transform.Position;
                toTarget.y = 0f;

                float stop = math.max(0.05f, targetPosition.StoppingRadius);
                if (math.lengthsq(toTarget) <= stop * stop)
                {
                    reservationReachedTag.ValueRW = true;
                }
            }
        }
    }
}