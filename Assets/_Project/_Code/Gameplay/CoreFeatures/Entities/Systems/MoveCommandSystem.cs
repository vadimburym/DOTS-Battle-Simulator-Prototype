using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Systems
{
    [DisableAutoCreation]
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

            var mapsRw = SystemAPI.GetSingletonRW<GridRuntimeMapSingleton>();
            var occupiedMap = mapsRw.ValueRW.OccupiedMap;

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
                var footprints = new NativeArray<Footprint>(requestedCount, Allocator.Temp);
                var gridStates = new NativeArray<GridNavigationState>(requestedCount, Allocator.Temp);

                int validCount = 0;

                for (int i = 0; i < targets.Length; i++)
                {
                    Entity e = targets[i].Value;

                    if (!entityManager.Exists(e)) continue;
                    if (!entityManager.HasComponent<LocalTransform>(e)) continue;
                    if (!entityManager.HasComponent<TargetPosition>(e)) continue;
                    if (!entityManager.HasComponent<Footprint>(e)) continue;
                    if (!entityManager.HasComponent<GridNavigationState>(e)) continue;

                    entities[validCount] = e;
                    transforms[validCount] = entityManager.GetComponentData<LocalTransform>(e);
                    targetPositions[validCount] = entityManager.GetComponentData<TargetPosition>(e);
                    footprints[validCount] = entityManager.GetComponentData<Footprint>(e);
                    gridStates[validCount] = entityManager.GetComponentData<GridNavigationState>(e);
                    validCount++;
                }

                if (validCount == 0)
                {
                    entities.Dispose();
                    transforms.Dispose();
                    targetPositions.Dispose();
                    footprints.Dispose();
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

                float selectionRadius = 0f;
                for (int i = 0; i < validCount; i++)
                {
                    float d = math.distance(groupCenter, transforms[i].Position);
                    selectionRadius = math.max(selectionRadius, d / grid.CellSize);
                }

                var localClaims = new NativeHashMap<int2, Entity>(math.max(64, validCount * 8), Allocator.Temp);

                for (int i = 0; i < validCount; i++)
                {
                    int footprintX = math.max(1, footprints[i].FootprintX);
                    int footprintY = math.max(1, footprints[i].FootprintY);

                    float preserveFactor = selectionRadius <= PreserveShapeThreshold
                        ? math.saturate(1f - (selectionRadius / PreserveShapeThreshold))
                        : 0f;

                    float2 currentOffsetWorld = new float2(
                        transforms[i].Position.x - groupCenter.x,
                        transforms[i].Position.z - groupCenter.z);

                    float2 currentOffsetCells = currentOffsetWorld / grid.CellSize;
                    float2 preferredOffsetCells = currentOffsetCells * preserveFactor;
                    float2 preferredCellPos = new float2(destinationCell.x, destinationCell.y) + preferredOffsetCells;

                    if (!TryFindNearestValidOccupiedCellForUnit(
                            ref grid,
                            occupiedMap,
                            localClaims,
                            entities[i],
                            destinationCell,
                            preferredCellPos,
                            footprintX,
                            footprintY,
                            out int2 newOccupiedCell))
                    {
                        continue;
                    }

                    BattlefieldGridUtils.AddAreaToLocalClaims(
                        localClaims,
                        entities[i],
                        newOccupiedCell,
                        footprintX,
                        footprintY);

                    var gs = gridStates[i];
                    bool hadOld = gs.HasOccupiedCell != 0;
                    int2 oldOccupiedCell = gs.OccupiedCell;

                    if (!hadOld || !math.all(oldOccupiedCell == newOccupiedCell))
                    {
                        if (hadOld)
                        {
                            BattlefieldGridUtils.ReleaseAreaDirect(
                                occupiedMap,
                                entities[i],
                                oldOccupiedCell,
                                footprintX,
                                footprintY);
                        }

                        BattlefieldGridUtils.OccupyAreaDirect(
                            occupiedMap,
                            entities[i],
                            newOccupiedCell,
                            footprintX,
                            footprintY);

                        gs.OccupiedCell = newOccupiedCell;
                        gs.HasOccupiedCell = 1;
                        gridStates[i] = gs;
                    }

                    float3 worldCenter = BattlefieldGridUtils.FootprintToWorldCenter(
                        ref grid,
                        newOccupiedCell,
                        footprintX,
                        footprintY,
                        transforms[i].Position.y);

                    targetPositions[i] = new TargetPosition
                    {
                        Position = worldCenter,
                        StoppingRadius = DefaultStoppingRadius
                    };
                }

                localClaims.Dispose();

                for (int i = 0; i < validCount; i++)
                {
                    entityManager.SetComponentData(entities[i], targetPositions[i]);
                    entityManager.SetComponentData(entities[i], gridStates[i]);
                }

                entities.Dispose();
                transforms.Dispose();
                targetPositions.Dispose();
                footprints.Dispose();
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
        private bool TryFindNearestValidOccupiedCellForUnit(
            ref BattlefieldGridBlob grid,
            NativeParallelHashMap<int2, Entity> occupiedMap,
            NativeHashMap<int2, Entity> localClaims,
            Entity entity,
            int2 destinationCell,
            float2 preferredCellPos,
            int footprintX,
            int footprintY,
            out int2 occupiedCell)
        {
            occupiedCell = default;

            if (TryTakeCell(
                    ref grid,
                    occupiedMap,
                    localClaims,
                    entity,
                    destinationCell,
                    footprintX,
                    footprintY,
                    ref occupiedCell))
            {
                return true;
            }

            for (int ring = 1; ring <= MaxValidationRings; ring++)
            {
                bool found = false;
                int2 bestCell = default;
                float bestDistSq = float.MaxValue;

                int x = -ring;
                int y = -ring;

                for (; x < ring; x++)
                {
                    EvaluateCellInRing(
                        ref grid, occupiedMap, localClaims, entity,
                        destinationCell + new int2(x * SlotSpacingCells, y * RowSpacingCells),
                        preferredCellPos, footprintX, footprintY,
                        ref found, ref bestCell, ref bestDistSq);
                }

                for (; y < ring; y++)
                {
                    EvaluateCellInRing(
                        ref grid, occupiedMap, localClaims, entity,
                        destinationCell + new int2(x * SlotSpacingCells, y * RowSpacingCells),
                        preferredCellPos, footprintX, footprintY,
                        ref found, ref bestCell, ref bestDistSq);
                }

                for (; x > -ring; x--)
                {
                    EvaluateCellInRing(
                        ref grid, occupiedMap, localClaims, entity,
                        destinationCell + new int2(x * SlotSpacingCells, y * RowSpacingCells),
                        preferredCellPos, footprintX, footprintY,
                        ref found, ref bestCell, ref bestDistSq);
                }

                for (; y > -ring; y--)
                {
                    EvaluateCellInRing(
                        ref grid, occupiedMap, localClaims, entity,
                        destinationCell + new int2(x * SlotSpacingCells, y * RowSpacingCells),
                        preferredCellPos, footprintX, footprintY,
                        ref found, ref bestCell, ref bestDistSq);
                }

                if (found)
                {
                    occupiedCell = bestCell;
                    return true;
                }
            }

            return false;
        }

        [BurstCompile]
        private bool TryTakeCell(
            ref BattlefieldGridBlob grid,
            NativeParallelHashMap<int2, Entity> occupiedMap,
            NativeHashMap<int2, Entity> localClaims,
            Entity entity,
            int2 candidateCell,
            int footprintX,
            int footprintY,
            ref int2 occupiedCell)
        {
            if (!BattlefieldGridUtils.IsAreaWalkable(ref grid, candidateCell, footprintX, footprintY))
                return false;

            if (!BattlefieldGridUtils.IsAreaFreeInOccupiedMap(
                    occupiedMap, candidateCell, footprintX, footprintY, entity))
                return false;

            if (!BattlefieldGridUtils.IsAreaFreeInLocalClaims(
                    localClaims, candidateCell, footprintX, footprintY, entity))
                return false;

            occupiedCell = candidateCell;
            return true;
        }

        [BurstCompile]
        private void EvaluateCellInRing(
            ref BattlefieldGridBlob grid,
            NativeParallelHashMap<int2, Entity> occupiedMap,
            NativeHashMap<int2, Entity> localClaims,
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

            if (!BattlefieldGridUtils.IsAreaFreeInOccupiedMap(
                    occupiedMap, candidateCell, footprintX, footprintY, entity))
                return;

            if (!BattlefieldGridUtils.IsAreaFreeInLocalClaims(
                    localClaims, candidateCell, footprintX, footprintY, entity))
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