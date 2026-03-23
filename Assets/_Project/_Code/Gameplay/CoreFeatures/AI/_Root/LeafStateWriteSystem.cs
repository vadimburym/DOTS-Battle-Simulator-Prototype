using _Project._Code.Gameplay.CoreFeatures.Entities.AiSystems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using VadimBurym.DodBehaviourTree;

namespace _Project._Code.Gameplay.CoreFeatures.AI._Root
{
    [DisableAutoCreation]
    [BurstCompile]
    [UpdateBefore(typeof(BehaviourTreeTickSystem))]
    public partial struct LeafStateWriteSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (
                         request,
                         entity) in
                     SystemAPI.Query<
                         RefRO<LeafStateWriteRequest>>()
                         .WithEntityAccess())
            {
                var buffer = SystemAPI.GetBuffer<LeafStateElement>(request.ValueRO.Entity);
                var index = request.ValueRO.Index;
                var data = buffer[index];
                data.StateEntity = request.ValueRO.Value;
                buffer[index] = data;
                ecb.DestroyEntity(entity);
            }
            ecb.Playback(state.EntityManager);
        }
    }
}