using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Systems
{
    [DisableAutoCreation]
    [UpdateAfter(typeof(GridRuntimeMapSystem))]
    [BurstCompile]
    public partial struct MoveCommandSystem : ISystem
    {
        private const float DefaultStoppingRadius = 0.1f;
        
        private const int SlotSpacingCells = 1;
        private const int RowSpacingCells = 1;

        private const int MaxValidationRings = 128;
        private const float PreserveShapeThreshold = 6f;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BattlefieldGridSingleton>();
            state.RequireForUpdate<GridRuntimeMapSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entityManager = state.EntityManager;

            var gridSingleton = SystemAPI.GetSingleton<BattlefieldGridSingleton>();
            ref var grid = ref gridSingleton.Value.Value;

            var maps = SystemAPI.GetSingleton<GridRuntimeMapSingleton>();
            var occupiedMap = maps.OccupiedMap;
            var reservedMap = maps.ReservedMap;

            var requestsToDestroy = new NativeList<Entity>(Allocator.Temp);

            foreach (var (
                         request,
                         targets,
                         requestEntity) in
                     SystemAPI.Query<
                             RefRO<MoveCommandRequest>,
                             DynamicBuffer<MoveCommandTarget>>()
                         .WithEntityAccess())
            {
                int requestedCount = targets.Length;
                if (requestedCount == 0)
                {
                    requestsToDestroy.Add(requestEntity);
                    continue;
                }

                var entities = new NativeArray<Entity>(requestedCount, Allocator.Temp);
                var transforms = new NativeArray<LocalTransform>(requestedCount, Allocator.Temp);
                var targetPositions = new NativeArray<TargetPosition>(requestedCount, Allocator.Temp);
                var unitBodies = new NativeArray<UnitBody>(requestedCount, Allocator.Temp);
                var gridStates = new NativeArray<GridNavigationState>(requestedCount, Allocator.Temp);

                int validCount = 0;

                for (int i = 0; i < targets.Length; i++)
                {
                    Entity e = targets[i].Value;

                    if (!entityManager.Exists(e))
                        continue;
                    if (!entityManager.HasComponent<LocalTransform>(e))
                        continue;
                    if (!entityManager.HasComponent<TargetPosition>(e))
                        continue;
                    if (!entityManager.HasComponent<UnitBody>(e))
                        continue;
                    if (!entityManager.HasComponent<GridNavigationState>(e))
                        continue;

                    entities[validCount] = e;
                    transforms[validCount] = entityManager.GetComponentData<LocalTransform>(e);
                    targetPositions[validCount] = entityManager.GetComponentData<TargetPosition>(e);
                    unitBodies[validCount] = entityManager.GetComponentData<UnitBody>(e);
                    gridStates[validCount] = entityManager.GetComponentData<GridNavigationState>(e);
                    validCount++;
                }

                if (validCount == 0)
                {
                    entities.Dispose();
                    transforms.Dispose();
                    targetPositions.Dispose();
                    unitBodies.Dispose();
                    gridStates.Dispose();

                    requestsToDestroy.Add(requestEntity);
                    continue;
                }

                float3 destination = request.ValueRO.Destination;
                int2 destinationCell = BattlefieldGridUtils.WorldToCell(ref grid, destination);

                float3 groupCenter = float3.zero;
                for (int i = 0; i < validCount; i++)
                    groupCenter += transforms[i].Position;
                groupCenter /= validCount;

                float3 forward3 = destination - groupCenter;
                forward3.y = 0f;
                forward3 = math.normalizesafe(forward3, new float3(0f, 0f, 1f));

                float2 forward2 = math.normalizesafe(new float2(forward3.x, forward3.z), new float2(0f, 1f));
                float2 right2 = new float2(forward2.y, -forward2.x);

                float selectionRadius = 0f;
                for (int i = 0; i < validCount; i++)
                {
                    float d = math.distance(groupCenter, transforms[i].Position);
                    selectionRadius = math.max(selectionRadius, d / grid.CellSize);
                }

                // Для каждого юнита ищем ближайшую валидную клетку.
                for (int i = 0; i < validCount; i++)
                {
                    int footprintX = math.max(1, unitBodies[i].FootprintX);
                    int footprintY = math.max(1, unitBodies[i].FootprintY);

                    float preserveFactor = selectionRadius <= PreserveShapeThreshold
                        ? math.saturate(1f - (selectionRadius / PreserveShapeThreshold))
                        : 0f;

                    float2 currentOffsetWorld = new float2(
                        transforms[i].Position.x - groupCenter.x,
                        transforms[i].Position.z - groupCenter.z);

                    float2 currentOffsetCells = currentOffsetWorld / grid.CellSize;
                    float2 preferredOffsetCells = currentOffsetCells * preserveFactor;

                    float2 preferredCellPos = new float2(destinationCell.x, destinationCell.y) + preferredOffsetCells;

                    if (TryFindNearestValidCellForUnit(
                            ref grid,
                            occupiedMap,
                            reservedMap,
                            entities[i],
                            destinationCell,
                            preferredCellPos,
                            footprintX,
                            footprintY,
                            out int2 reservedCell))
                    {
                        if (gridStates[i].HasReservedCell != 0)
                        {
                            BattlefieldGridUtils.ReleaseArea(
                                reservedMap,
                                entities[i],
                                gridStates[i].ReservedCell,
                                footprintX,
                                footprintY);
                        }

                        BattlefieldGridUtils.ReserveArea(
                            reservedMap,
                            entities[i],
                            reservedCell,
                            footprintX,
                            footprintY);

                        float3 worldCenter = BattlefieldGridUtils.FootprintToWorldCenter(
                            ref grid,
                            reservedCell,
                            footprintX,
                            footprintY,
                            transforms[i].Position.y);

                        targetPositions[i] = new TargetPosition
                        {
                            Position = worldCenter,
                            StoppingRadius = DefaultStoppingRadius
                        };

                        gridStates[i] = new GridNavigationState
                        {
                            OccupiedCell = gridStates[i].OccupiedCell,
                            ReservedCell = reservedCell,
                            HasOccupiedCell = gridStates[i].HasOccupiedCell,
                            HasReservedCell = 1
                        };
                    }
                }

                for (int i = 0; i < validCount; i++)
                {
                    entityManager.SetComponentData(entities[i], targetPositions[i]);
                    entityManager.SetComponentData(entities[i], gridStates[i]);
                }

                entities.Dispose();
                transforms.Dispose();
                targetPositions.Dispose();
                unitBodies.Dispose();
                gridStates.Dispose();

                requestsToDestroy.Add(requestEntity);
            }

            for (int i = 0; i < requestsToDestroy.Length; i++)
            {
                if (entityManager.Exists(requestsToDestroy[i]))
                    entityManager.DestroyEntity(requestsToDestroy[i]);
            }

            requestsToDestroy.Dispose();
        }

        [BurstCompile]
        private bool TryFindNearestValidCellForUnit(
            ref BattlefieldGridBlob grid,
            NativeParallelHashMap<int2, Entity> occupiedMap,
            NativeParallelHashMap<int2, Entity> reservedMap,
            Entity entity,
            int2 destinationCell,
            float2 preferredCellPos,
            int footprintX,
            int footprintY,
            out int2 reservedCell)
        {
            reservedCell = default;

            // ring = 0: сама точка назначения
            if (TryTakeCell(
                    ref grid,
                    occupiedMap,
                    reservedMap,
                    entity,
                    destinationCell,
                    footprintX,
                    footprintY,
                    ref reservedCell))
            {
                return true;
            }

            // Дальше расширяемся кольцами от destination.
            for (int ring = 1; ring <= MaxValidationRings; ring++)
            {
                bool foundInThisRing = false;
                int2 ringBestCell = default;
                float ringBestDistSq = float.MaxValue;

                int x = -ring;
                int y = -ring;

                for (; x < ring; x++)
                {
                    EvaluateCellInRing(
                        ref grid,
                        occupiedMap,
                        reservedMap,
                        entity,
                        destinationCell + new int2(x * SlotSpacingCells, y * RowSpacingCells),
                        preferredCellPos,
                        footprintX,
                        footprintY,
                        ref foundInThisRing,
                        ref ringBestCell,
                        ref ringBestDistSq);
                }

                for (; y < ring; y++)
                {
                    EvaluateCellInRing(
                        ref grid,
                        occupiedMap,
                        reservedMap,
                        entity,
                        destinationCell + new int2(x * SlotSpacingCells, y * RowSpacingCells),
                        preferredCellPos,
                        footprintX,
                        footprintY,
                        ref foundInThisRing,
                        ref ringBestCell,
                        ref ringBestDistSq);
                }

                for (; x > -ring; x--)
                {
                    EvaluateCellInRing(
                        ref grid,
                        occupiedMap,
                        reservedMap,
                        entity,
                        destinationCell + new int2(x * SlotSpacingCells, y * RowSpacingCells),
                        preferredCellPos,
                        footprintX,
                        footprintY,
                        ref foundInThisRing,
                        ref ringBestCell,
                        ref ringBestDistSq);
                }

                for (; y > -ring; y--)
                {
                    EvaluateCellInRing(
                        ref grid,
                        occupiedMap,
                        reservedMap,
                        entity,
                        destinationCell + new int2(x * SlotSpacingCells, y * RowSpacingCells),
                        preferredCellPos,
                        footprintX,
                        footprintY,
                        ref foundInThisRing,
                        ref ringBestCell,
                        ref ringBestDistSq);
                }

                if (foundInThisRing)
                {
                    reservedCell = ringBestCell;
                    return true;
                }
            }

            return false;
        }

        [BurstCompile]
        private bool TryTakeCell(
            ref BattlefieldGridBlob grid,
            NativeParallelHashMap<int2, Entity> occupiedMap,
            NativeParallelHashMap<int2, Entity> reservedMap,
            Entity entity,
            int2 candidateCell,
            int footprintX,
            int footprintY,
            ref int2 reservedCell)
        {
            if (!BattlefieldGridUtils.IsAreaWalkable(ref grid, candidateCell, footprintX, footprintY))
                return false;

            if (!BattlefieldGridUtils.IsAreaFree(
                    occupiedMap,
                    reservedMap,
                    candidateCell,
                    footprintX,
                    footprintY,
                    entity))
                return false;

            reservedCell = candidateCell;
            return true;
        }

        [BurstCompile]
        private void EvaluateCellInRing(
            ref BattlefieldGridBlob grid,
            NativeParallelHashMap<int2, Entity> occupiedMap,
            NativeParallelHashMap<int2, Entity> reservedMap,
            Entity entity,
            int2 candidateCell,
            float2 preferredCellPos,
            int footprintX,
            int footprintY,
            ref bool found,
            ref int2 bestCell,
            ref float bestDistSq)
        {
            if (!BattlefieldGridUtils.IsAreaWalkable(ref grid, candidateCell, footprintX, footprintY))
                return;

            if (!BattlefieldGridUtils.IsAreaFree(
                    occupiedMap,
                    reservedMap,
                    candidateCell,
                    footprintX,
                    footprintY,
                    entity))
                return;

            float2 candidatePos = candidateCell;
            float d = math.lengthsq(preferredCellPos - candidatePos);

            if (d < bestDistSq)
            {
                bestDistSq = d;
                bestCell = candidateCell;
                found = true;
            }
        }
    }
}