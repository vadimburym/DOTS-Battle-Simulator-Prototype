using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Systems
{
    [DisableAutoCreation]
    public partial struct MoveCommandSystem : ISystem
    {
        private const float SlotSpacing = 1.6f;
        private const float RowSpacing = 1.6f;
        private const int MaxValidationRings = 64;
        private const float PreserveShapeThreshold = 6f;
        private const float GroundRayHeight = 50f;
        private const float GroundRayDistance = 200f;

        private LayerMask _groundMask;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MoveCommandSingleton>();
            _groundMask = LayerMask.GetMask("Ground");
        }

        public void OnUpdate(ref SystemState state)
        {
            var command = SystemAPI.GetSingleton<MoveCommandSingleton>();
            if (command.IsIssued == 0)
                return;

            var selectedQuery = SystemAPI.QueryBuilder()
                .WithAll<Selected, TargetPosition, LocalTransform>()
                .Build();

            var count = selectedQuery.CalculateEntityCount();
            if (count == 0)
            {
                command.IsIssued = 0;
                SystemAPI.SetSingleton(command);
                return;
            }

            var entities = selectedQuery.ToEntityArray(Allocator.Temp);
            var transforms = selectedQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            var targetPositions = selectedQuery.ToComponentDataArray<TargetPosition>(Allocator.Temp);

            float3 groupCenter = float3.zero;
            for (int i = 0; i < transforms.Length; i++)
                groupCenter += transforms[i].Position;
            groupCenter /= transforms.Length;

            float3 forward = command.Destination - groupCenter;
            forward.y = 0f;
            forward = math.normalizesafe(forward, new float3(0f, 0f, 1f));

            float3 right = math.normalizesafe(math.cross(math.up(), forward), new float3(1f, 0f, 0f));

            float selectionRadius = 0f;
            for (int i = 0; i < transforms.Length; i++)
            {
                float d = math.distance(groupCenter, transforms[i].Position);
                selectionRadius = math.max(selectionRadius, d);
            }

            var validSlots = new NativeList<float3>(count, Allocator.Temp);
            GenerateValidSlots(command.Destination, right, forward, count, ref validSlots);

            if (validSlots.Length == 0)
            {
                command.IsIssued = 0;
                SystemAPI.SetSingleton(command);
                return;
            }

            var slotUsed = new NativeArray<byte>(validSlots.Length, Allocator.Temp);

            // Greedy nearest assignment, optionally with slight shape preservation.
            for (int i = 0; i < entities.Length; i++)
            {
                float3 currentPos = transforms[i].Position;

                float preserveFactor = selectionRadius <= PreserveShapeThreshold
                    ? math.saturate(1f - (selectionRadius / PreserveShapeThreshold))
                    : 0f;

                float3 preferredOffset = (currentPos - groupCenter) * preserveFactor;
                preferredOffset.y = 0f;
                float3 preferredPos = command.Destination + preferredOffset;

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
                    targetPositions[i] = new TargetPosition { Value = validSlots[bestSlot] };
                }
            }

            var entityManager = state.EntityManager;
            for (int i = 0; i < entities.Length; i++)
                entityManager.SetComponentData(entities[i], targetPositions[i]);

            command.IsIssued = 0;
            SystemAPI.SetSingleton(command);

            entities.Dispose();
            transforms.Dispose();
            targetPositions.Dispose();
            validSlots.Dispose();
            slotUsed.Dispose();
        }

        private void GenerateValidSlots(
            float3 destination,
            float3 right,
            float3 forward,
            int neededCount,
            ref NativeList<float3> validSlots)
        {
            if (TryProjectToGround(destination, out var centerGround))
                validSlots.Add(centerGround);

            for (int ring = 1; ring <= MaxValidationRings && validSlots.Length < neededCount; ring++)
            {
                int x = -ring;
                int y = -ring;

                // bottom edge
                for (; x < ring; x++)
                    TryAddCell(x, y, destination, right, forward, ref validSlots, neededCount);

                // right edge
                for (; y < ring; y++)
                    TryAddCell(x, y, destination, right, forward, ref validSlots, neededCount);

                // top edge
                for (; x > -ring; x--)
                    TryAddCell(x, y, destination, right, forward, ref validSlots, neededCount);

                // left edge
                for (; y > -ring; y--)
                    TryAddCell(x, y, destination, right, forward, ref validSlots, neededCount);
            }
        }

        private void TryAddCell(
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

            if (TryProjectToGround(candidate, out var groundPoint))
                validSlots.Add(groundPoint);
        }

        private bool TryProjectToGround(float3 candidate, out float3 groundPoint)
        {
            var origin = new Vector3(candidate.x, candidate.y + GroundRayHeight, candidate.z);
            if (Physics.Raycast(origin, Vector3.down, out var hit, GroundRayDistance, _groundMask))
            {
                groundPoint = hit.point;
                return true;
            }

            groundPoint = default;
            return false;
        }
    }
}