using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Systems
{
    [DisableAutoCreation]
    public partial struct MovementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (localTransform,
                          movement) in
                     SystemAPI.Query<
                         RefRW<LocalTransform>,
                         RefRO<MovementComponent>>())
            {
                localTransform.ValueRW.Position = localTransform.ValueRO.Position + movement.ValueRO.Direction * (movement.ValueRO.Speed * deltaTime);
            }
        }
    }
}