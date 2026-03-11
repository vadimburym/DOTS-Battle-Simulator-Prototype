using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Systems
{
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
                if (math.lengthsq(direction) < EPS_SQ)
                {
                    physicsVelocity.Linear = float3.zero;
                    physicsVelocity.Angular = float3.zero;
                    return;
                }
                direction = math.normalize(direction);
                localTransform.Rotation = math.slerp(
                    localTransform.Rotation,
                    quaternion.LookRotation(direction, math.up()),
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
}