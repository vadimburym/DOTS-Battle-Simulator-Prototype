using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Project._Code.Gameplay.CoreFeatures
{
    [BurstCompile]
    [DisableAutoCreation]
    public partial struct GridOccupancySyncSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BattlefieldGridSingleton>();
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
                in LocalTransform transform,
                in UnitBody body,
                ref GridNavigationState gridState)
            {
                ref var grid = ref GridRef.Value;

                int2 occupiedCell = BattlefieldGridUtils.WorldToCell(ref grid, transform.Position);

                gridState.OccupiedCell = occupiedCell;
                gridState.HasOccupiedCell = 1;

                if (gridState.HasReservedCell != 0)
                {
                    float3 reservedCenter = BattlefieldGridUtils.FootprintToWorldCenter(
                        ref grid,
                        gridState.ReservedCell,
                        body.FootprintX,
                        body.FootprintY,
                        transform.Position.y);

                    float3 toReserved = reservedCenter - transform.Position;
                    toReserved.y = 0f;

                    if (math.lengthsq(toReserved) <= 0.05f * 0.05f)
                    {
                        gridState.HasReservedCell = 0;
                    }
                }
            }
        }
    }
}