using _Project._Code.Gameplay.CoreFeatures.Entities.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Project._Code.Gameplay.CoreFeatures
{
    [BurstCompile]
    [DisableAutoCreation]
    [UpdateAfter(typeof(MovementSystem))]
    public partial struct GridMovingCellSyncSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BattlefieldGridSingleton>();
            if (!SystemAPI.TryGetSingletonEntity<GridRuntimeMapSingleton>(out _))
            {
                var e = state.EntityManager.CreateEntity(typeof(GridRuntimeMapSingleton));
                state.EntityManager.SetComponentData(e, new GridRuntimeMapSingleton {
                    OccupiedMap = new NativeParallelHashMap<int2, Entity>(16384, Allocator.Persistent)
                });
            }
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonRW<GridRuntimeMapSingleton>(out var mapsRw))
                return;
            if (mapsRw.ValueRO.OccupiedMap.IsCreated)
                mapsRw.ValueRW.OccupiedMap.Dispose();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var gridRef = SystemAPI.GetSingleton<BattlefieldGridSingleton>().Value;
            state.Dependency = new GridMovingCellSyncJob {
                GridRef = gridRef
            }.ScheduleParallel(state.Dependency);
        }
        
        [BurstCompile]
        public partial struct GridMovingCellSyncJob : IJobEntity
        {
            public BlobAssetReference<BattlefieldGridBlob> GridRef;

            [BurstCompile]
            private void Execute(
                ref GridNavigationState gridState,
                in LocalTransform transform)
            {
                ref var grid = ref GridRef.Value;
                var position = transform.Position;
                var local = new float2(position.x - grid.Origin.x, position.z - grid.Origin.z);
                gridState.MovingCell = (int2)math.floor(local / grid.CellSize);
            }
        }
    }
}