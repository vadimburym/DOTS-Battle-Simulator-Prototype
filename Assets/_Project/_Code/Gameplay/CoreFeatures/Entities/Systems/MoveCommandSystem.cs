using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Systems
{
    [DisableAutoCreation]
    public partial struct MoveCommandSystem : ISystem
    {
        private const float StoppingRadius = 1f;
        
        private const float SlotSpacing = 1.6f;
        private const float RowSpacing = 1.6f;
        private const int MaxValidationRings = 64;
        private const float PreserveShapeThreshold = 6f;
        private const float GroundRayHeight = 50f;
        private const float GroundRayDistance = 200f;

        private CollisionFilter _groundFilter;

        public void OnCreate(ref SystemState state)
        {
            _groundFilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << 6,
                GroupIndex = 0
            };
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var entityManager = state.EntityManager;

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
                int count = targets.Length;
                if (count == 0)
                {
                    requestsToDestroy.Add(requestEntity);
                    continue;
                }

                var entities = new NativeArray<Entity>(count, Allocator.Temp);
                var transforms = new NativeArray<LocalTransform>(count, Allocator.Temp);
                var targetPositions = new NativeArray<TargetPosition>(count, Allocator.Temp);

                int validCount = 0;

                for (int i = 0; i < targets.Length; i++)
                {
                    var targetEntity = targets[i].Value;
                    
                    if (!entityManager.Exists(targetEntity))
                        continue;
                    if (!entityManager.HasComponent<LocalTransform>(targetEntity))
                        continue;
                    if (!entityManager.HasComponent<TargetPosition>(targetEntity))
                        continue;

                    entities[validCount] = targetEntity;
                    transforms[validCount] = entityManager.GetComponentData<LocalTransform>(targetEntity);
                    targetPositions[validCount] = entityManager.GetComponentData<TargetPosition>(targetEntity);
                    validCount++;
                }

                if (validCount == 0)
                {
                    entities.Dispose();
                    transforms.Dispose();
                    targetPositions.Dispose();
                    requestsToDestroy.Add(requestEntity);
                    continue;
                }

                var destination = request.ValueRO.Destination;

                var groupCenter = float3.zero;
                for (int i = 0; i < validCount; i++)
                    groupCenter += transforms[i].Position;
                groupCenter /= validCount;

                var forward = destination - groupCenter;
                forward.y = 0f;
                forward = math.normalizesafe(forward, new float3(0f, 0f, 1f));

                var right = math.normalizesafe(math.cross(math.up(), forward), new float3(1f, 0f, 0f));

                float selectionRadius = 0f;
                for (int i = 0; i < validCount; i++)
                {
                    float d = math.distance(groupCenter, transforms[i].Position);
                    selectionRadius = math.max(selectionRadius, d);
                }

                var validSlots = new NativeList<float3>(validCount, Allocator.Temp);
                GenerateValidSlots(physicsWorld, destination, right, forward, validCount, ref validSlots);

                if (validSlots.Length > 0)
                {
                    var slotUsed = new NativeArray<byte>(validSlots.Length, Allocator.Temp);

                    for (int i = 0; i < validCount; i++)
                    {
                        var currentPos = transforms[i].Position;

                        float preserveFactor = selectionRadius <= PreserveShapeThreshold
                            ? math.saturate(1f - (selectionRadius / PreserveShapeThreshold))
                            : 0f;

                        var preferredOffset = (currentPos - groupCenter) * preserveFactor;
                        preferredOffset.y = 0f;
                        var preferredPos = destination + preferredOffset;

                        int bestSlot = -1;
                        float bestDistSq = float.MaxValue;

                        for (int s = 0; s < validSlots.Length; s++)
                        {
                            if (slotUsed[s] != 0)
                                continue;

                            float d = math.distancesq(preferredPos, validSlots[s]);
                            if (d < bestDistSq)
                            {
                                bestDistSq = d;
                                bestSlot = s;
                            }
                        }

                        if (bestSlot >= 0)
                        {
                            slotUsed[bestSlot] = 1;
                            targetPositions[i] = new TargetPosition
                            {
                                Position = validSlots[bestSlot],
                                StoppingRadius = StoppingRadius
                            };
                        }
                    }

                    slotUsed.Dispose();

                    for (int i = 0; i < validCount; i++)
                        entityManager.SetComponentData(entities[i], targetPositions[i]);
                }
                validSlots.Dispose();
                entities.Dispose();
                transforms.Dispose();
                targetPositions.Dispose();
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
        private void GenerateValidSlots(
            in PhysicsWorldSingleton physicsWorld,
            float3 destination,
            float3 right,
            float3 forward,
            int neededCount,
            ref NativeList<float3> validSlots)
        {
            if (TryProjectToGround(physicsWorld, destination, out var centerGround))
                validSlots.Add(centerGround);

            for (int ring = 1; ring <= MaxValidationRings && validSlots.Length < neededCount; ring++)
            {
                int x = -ring;
                int y = -ring;

                for (; x < ring; x++)
                    TryAddCell(physicsWorld, x, y, destination, right, forward, ref validSlots, neededCount);

                for (; y < ring; y++)
                    TryAddCell(physicsWorld, x, y, destination, right, forward, ref validSlots, neededCount);

                for (; x > -ring; x--)
                    TryAddCell(physicsWorld, x, y, destination, right, forward, ref validSlots, neededCount);

                for (; y > -ring; y--)
                    TryAddCell(physicsWorld, x, y, destination, right, forward, ref validSlots, neededCount);
            }
        }

        [BurstCompile]
        private void TryAddCell(
            in PhysicsWorldSingleton physicsWorld,
            int cellX,
            int cellY,
            float3 destination,
            float3 right,
            float3 forward,
            ref NativeList<float3> validSlots,
            int neededCount)
        {
            if (validSlots.Length >= neededCount)
                return;

            float3 candidate =
                destination +
                right * (cellX * SlotSpacing) +
                forward * (cellY * RowSpacing);

            if (TryProjectToGround(physicsWorld, candidate, out var groundPoint))
                validSlots.Add(groundPoint);
        }

        [BurstCompile]
        private bool TryProjectToGround(
            in PhysicsWorldSingleton physicsWorld,
            float3 candidate,
            out float3 groundPoint)
        {
            var input = new RaycastInput
            {
                Start = candidate + new float3(0f, GroundRayHeight, 0f),
                End = candidate - new float3(0f, GroundRayDistance, 0f),
                Filter = _groundFilter
            };

            if (physicsWorld.CastRay(input, out var hit))
            {
                groundPoint = math.lerp(input.Start, input.End, hit.Fraction);
                return true;
            }

            groundPoint = default;
            return false;
        }
    }
}