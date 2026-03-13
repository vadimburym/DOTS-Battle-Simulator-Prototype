using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace _Project._Code.Gameplay.CoreFeatures
{
    [DisableAutoCreation]
    [BurstCompile]
    public partial struct GridRuntimeMapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BattlefieldGridSingleton>();

            if (!SystemAPI.TryGetSingletonEntity<GridRuntimeMapSingleton>(out _))
            {
                var entity = state.EntityManager.CreateEntity(typeof(GridRuntimeMapSingleton));
                state.EntityManager.SetComponentData(entity, new GridRuntimeMapSingleton {
                    OccupiedMap = new NativeParallelHashMap<int2, Entity>(16384, Allocator.Persistent),
                    ReservedMap = new NativeParallelHashMap<int2, Entity>(16384, Allocator.Persistent)
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

            if (mapsRw.ValueRO.ReservedMap.IsCreated)
                mapsRw.ValueRW.ReservedMap.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var mapsRw = SystemAPI.GetSingletonRW<GridRuntimeMapSingleton>();

            int unitCount = SystemAPI.QueryBuilder()
                .WithAll<GridNavigationState, UnitBody>()
                .Build()
                .CalculateEntityCount();

            int desiredCapacity = math.max(1024, unitCount * 4);

            if (mapsRw.ValueRO.OccupiedMap.Capacity < desiredCapacity)
                mapsRw.ValueRW.OccupiedMap.Capacity = desiredCapacity;

            mapsRw.ValueRW.OccupiedMap.Clear();

            var buildOccupiedJob = new BuildOccupiedMapJob {
                OccupiedWriter = mapsRw.ValueRW.OccupiedMap.AsParallelWriter()
            };

            state.Dependency = buildOccupiedJob.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();
        }

        [BurstCompile]
        public partial struct BuildOccupiedMapJob : IJobEntity
        {
            public NativeParallelHashMap<int2, Entity>.ParallelWriter OccupiedWriter;

            [BurstCompile]
            private void Execute(
                Entity entity,
                in GridNavigationState gridState,
                in UnitBody body)
            {
                if (gridState.HasOccupiedCell == 0)
                    return;

                int footprintX = math.max(1, body.FootprintX);
                int footprintY = math.max(1, body.FootprintY);

                if (footprintX == 1 && footprintY == 1)
                {
                    OccupiedWriter.TryAdd(gridState.OccupiedCell, entity);
                    return;
                }

                for (int y = 0; y < footprintY; y++)
                {
                    for (int x = 0; x < footprintX; x++)
                    {
                        int2 cell = gridState.OccupiedCell + new int2(x, y);
                        OccupiedWriter.TryAdd(cell, entity);
                    }
                }
            }
        }
    }
}