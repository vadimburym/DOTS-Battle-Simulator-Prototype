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
    public partial struct MoveCommandSystem_Test : ISystem
    {
        private const float DefaultStoppingRadius = 0.1f;
        
        private const float SlotSpacing = 1.6f;
        private const float RowSpacing = 1.6f;

        private const int MaxValidationRings = 128;
        private const float PreserveShapeThreshold = 6f;

        private NativeParallelHashMap<int2, Entity> _occupiedMap;
        private NativeParallelHashMap<int2, Entity> _reservedMap;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BattlefieldGridSingleton>();

            _occupiedMap = new NativeParallelHashMap<int2, Entity>(16384, Allocator.Persistent);
            _reservedMap = new NativeParallelHashMap<int2, Entity>(16384, Allocator.Persistent);
        }

        public void OnDestroy(ref SystemState state)
        {
            if (_occupiedMap.IsCreated)
                _occupiedMap.Dispose();

            if (_reservedMap.IsCreated)
                _reservedMap.Dispose();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            var gridSingleton = SystemAPI.GetSingleton<BattlefieldGridSingleton>();
            ref var grid = ref gridSingleton.Value.Value;

            RebuildRuntimeMaps(ref state);

            var requestsToDestroy = new NativeList<Entity>(Allocator.Temp);

            foreach (var (request, targets, requestEntity) in
                     SystemAPI.Query<RefRO<MoveCommandRequest>, DynamicBuffer<MoveCommandTarget>>()
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

                float3 groupCenter = float3.zero;
                for (int i = 0; i < validCount; i++)
                    groupCenter += transforms[i].Position;
                groupCenter /= validCount;

                float3 forward = destination - groupCenter;
                forward.y = 0f;
                forward = math.normalizesafe(forward, new float3(0f, 0f, 1f));

                float3 right = math.normalizesafe(math.cross(math.up(), forward), new float3(1f, 0f, 0f));

                float selectionRadius = 0f;
                for (int i = 0; i < validCount; i++)
                {
                    float d = math.distance(groupCenter, transforms[i].Position);
                    selectionRadius = math.max(selectionRadius, d);
                }
                
                for (int i = 0; i < validCount; i++)
                {
                    float3 currentPos = transforms[i].Position;

                    float preserveFactor = selectionRadius <= PreserveShapeThreshold
                        ? math.saturate(1f - (selectionRadius / PreserveShapeThreshold))
                        : 0f;

                    float3 preferredOffset = (currentPos - groupCenter) * preserveFactor;
                    preferredOffset.y = 0f;
                    float3 preferredPos = destination + preferredOffset;

                    int footprintX = math.max(1, unitBodies[i].FootprintX);
                    int footprintY = math.max(1, unitBodies[i].FootprintY);

                    if (TryFindNearestValidSlotForUnit(
                            ref grid,
                            entities[i],
                            destination,
                            right,
                            forward,
                            preferredPos,
                            footprintX,
                            footprintY,
                            out float3 reservedWorldPos,
                            out int2 reservedCell))
                    {
                        BattlefieldGridUtils.ReserveArea(
                            _reservedMap,
                            entities[i],
                            reservedCell,
                            footprintX,
                            footprintY);

                        targetPositions[i] = new TargetPosition
                        {
                            Position = reservedWorldPos,
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
        private void RebuildRuntimeMaps(ref SystemState state)
        {
            _occupiedMap.Clear();
            _reservedMap.Clear();

            int unitCount = SystemAPI.QueryBuilder()
                .WithAll<GridNavigationState, UnitBody>()
                .Build()
                .CalculateEntityCount();

            int desiredCapacity = math.max(1024, unitCount * 4);

            if (_occupiedMap.Capacity < desiredCapacity)
                _occupiedMap.Capacity = desiredCapacity;

            if (_reservedMap.Capacity < desiredCapacity)
                _reservedMap.Capacity = desiredCapacity;

            foreach (var (gridState, body, entity) in
                     SystemAPI.Query<RefRO<GridNavigationState>, RefRO<UnitBody>>()
                         .WithEntityAccess())
            {
                int footprintX = math.max(1, body.ValueRO.FootprintX);
                int footprintY = math.max(1, body.ValueRO.FootprintY);

                if (gridState.ValueRO.HasOccupiedCell != 0)
                {
                    BattlefieldGridUtils.OccupyArea(
                        _occupiedMap,
                        entity,
                        gridState.ValueRO.OccupiedCell,
                        footprintX,
                        footprintY);
                }

                if (gridState.ValueRO.HasReservedCell != 0)
                {
                    BattlefieldGridUtils.ReserveArea(
                        _reservedMap,
                        entity,
                        gridState.ValueRO.ReservedCell,
                        footprintX,
                        footprintY);
                }
            }
        }

        [BurstCompile]
        private bool TryFindNearestValidSlotForUnit(
            ref BattlefieldGridBlob grid,
            Entity entity,
            float3 destination,
            float3 right,
            float3 forward,
            float3 preferredPos,
            int footprintX,
            int footprintY,
            out float3 reservedWorldPos,
            out int2 reservedCell)
        {
            reservedWorldPos = default;
            reservedCell = default;
            
            if (TryTakeCandidate(
                    ref grid,
                    entity,
                    destination,
                    preferredPos,
                    footprintX,
                    footprintY,
                    ref reservedWorldPos,
                    ref reservedCell))
            {
                return true;
            }

            for (int ring = 1; ring <= MaxValidationRings; ring++)
            {
                bool foundInThisRing = false;
                float3 bestWorldPos = default;
                int2 bestCell = default;
                float bestDistSq = float.MaxValue;

                int x = -ring;
                int y = -ring;

                for (; x < ring; x++)
                {
                    EvaluateCandidateInRing(
                        ref grid,
                        entity,
                        destination + right * (x * SlotSpacing) + forward * (y * RowSpacing),
                        preferredPos,
                        footprintX,
                        footprintY,
                        ref foundInThisRing,
                        ref bestWorldPos,
                        ref bestCell,
                        ref bestDistSq);
                }

                for (; y < ring; y++)
                {
                    EvaluateCandidateInRing(
                        ref grid,
                        entity,
                        destination + right * (x * SlotSpacing) + forward * (y * RowSpacing),
                        preferredPos,
                        footprintX,
                        footprintY,
                        ref foundInThisRing,
                        ref bestWorldPos,
                        ref bestCell,
                        ref bestDistSq);
                }

                for (; x > -ring; x--)
                {
                    EvaluateCandidateInRing(
                        ref grid,
                        entity,
                        destination + right * (x * SlotSpacing) + forward * (y * RowSpacing),
                        preferredPos,
                        footprintX,
                        footprintY,
                        ref foundInThisRing,
                        ref bestWorldPos,
                        ref bestCell,
                        ref bestDistSq);
                }

                for (; y > -ring; y--)
                {
                    EvaluateCandidateInRing(
                        ref grid,
                        entity,
                        destination + right * (x * SlotSpacing) + forward * (y * RowSpacing),
                        preferredPos,
                        footprintX,
                        footprintY,
                        ref foundInThisRing,
                        ref bestWorldPos,
                        ref bestCell,
                        ref bestDistSq);
                }

                if (foundInThisRing)
                {
                    reservedWorldPos = bestWorldPos;
                    reservedCell = bestCell;
                    return true;
                }
            }

            return false;
        }

        [BurstCompile]
        private bool TryTakeCandidate(
            ref BattlefieldGridBlob grid,
            Entity entity,
            float3 candidateWorld,
            float3 preferredPos,
            int footprintX,
            int footprintY,
            ref float3 reservedWorldPos,
            ref int2 reservedCell)
        {
            int2 anchorCell = BattlefieldGridUtils.WorldToCell(ref grid, candidateWorld);

            if (!BattlefieldGridUtils.IsAreaWalkable(ref grid, anchorCell, footprintX, footprintY))
                return false;

            if (!BattlefieldGridUtils.IsAreaFree(
                    _occupiedMap,
                    _reservedMap,
                    anchorCell,
                    footprintX,
                    footprintY,
                    entity))
                return false;

            reservedWorldPos = candidateWorld;
            reservedCell = anchorCell;
            return true;
        }

        [BurstCompile]
        private void EvaluateCandidateInRing(
            ref BattlefieldGridBlob grid,
            Entity entity,
            float3 candidateWorld,
            float3 preferredPos,
            int footprintX,
            int footprintY,
            ref bool found,
            ref float3 bestWorldPos,
            ref int2 bestCell,
            ref float bestDistSq)
        {
            int2 anchorCell = BattlefieldGridUtils.WorldToCell(ref grid, candidateWorld);

            if (!BattlefieldGridUtils.IsAreaWalkable(ref grid, anchorCell, footprintX, footprintY))
                return;

            if (!BattlefieldGridUtils.IsAreaFree(
                    _occupiedMap,
                    _reservedMap,
                    anchorCell,
                    footprintX,
                    footprintY,
                    entity))
                return;

            float d = math.distancesq(preferredPos, candidateWorld);
            if (d < bestDistSq)
            {
                bestDistSq = d;
                bestWorldPos = candidateWorld;
                bestCell = anchorCell;
                found = true;
            }
        }
    }
}