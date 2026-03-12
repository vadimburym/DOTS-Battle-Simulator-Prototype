using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Systems
{
    [DisableAutoCreation]
    public partial struct MovementSystem : ISystem
    {
        [BurstCompile]
        public partial struct MovementJob : IJobEntity
        {
            public float DeltaTime;

            [BurstCompile]
            public void Execute(
                ref LocalTransform localTransform,
                in MovementComponent movement,
                in TargetPosition targetPosition)
            {
                float3 toTarget = targetPosition.Position - localTransform.Position;
                toTarget.y = 0f;
                float stoppingRadius = math.max(0.05f, targetPosition.StoppingRadius);
                float stoppingRadiusSq = stoppingRadius * stoppingRadius;
                float distSq = math.lengthsq(toTarget);
                if (distSq <= stoppingRadiusSq)
                    return;
                float3 dir = math.normalizesafe(toTarget, new float3(0f, 0f, 1f));
                quaternion desiredRotation = quaternion.LookRotationSafe(dir, math.up());

                localTransform.Rotation = math.slerp(
                    localTransform.Rotation,
                    desiredRotation,
                    math.saturate(DeltaTime * movement.RotationSpeed));

                float distance = math.sqrt(distSq);
                float moveStep = movement.Speed * DeltaTime;
                float travel = math.min(moveStep, math.max(0f, distance - stoppingRadius));
                localTransform.Position += dir * travel;
            }
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new MovementJob {
                DeltaTime = SystemAPI.Time.DeltaTime
            };
            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }
    /*
    [DisableAutoCreation]
    public partial struct MovementSystem : ISystem
    {
        public const float EPS_SQ = 1f;
        
        [BurstCompile]
        public partial struct MovementJob : IJobEntity
        {
            public float DeltaTime;
            
            [BurstCompile]
            public void Execute(
                ref LocalTransform localTransform,
                in MovementComponent movement,
                in TargetPosition targetPosition,
                ref PhysicsVelocity physicsVelocity)
            {
                var direction = targetPosition.Value - localTransform.Position;
                if (math.lengthsq(direction) < movement.StoppingRadiusSq)
                {
                    physicsVelocity.Linear = float3.zero;
                    physicsVelocity.Angular = float3.zero;
                    return;
                }
                direction = math.normalize(direction);
                localTransform.Rotation = math.slerp(
                    localTransform.Rotation,
                    quaternion.LookRotationSafe(direction, math.up()),
                    DeltaTime * movement.RotationSpeed);
                physicsVelocity.Linear = direction * movement.Speed;
                physicsVelocity.Angular = float3.zero;
            }
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var movementJob = new MovementJob {
                DeltaTime = SystemAPI.Time.DeltaTime
            };
            movementJob.ScheduleParallel();
        }
    }
    */
}