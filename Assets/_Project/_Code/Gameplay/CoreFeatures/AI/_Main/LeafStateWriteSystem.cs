using _Project._Code.Gameplay.CoreFeatures.Entities.AiSystems;
using _Project._Code.Infrastructure.EcsContext;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using VadimBurym.DodBehaviourTree;

namespace _Project._Code.Gameplay.CoreFeatures.AI._Root
{
    [DisableAutoCreation]
    [BurstCompile]
    [UpdateBefore(typeof(BehaviourTreeTickSystem))]
    //[UpdateInGroup(typeof(LocalSystemsGroup), OrderFirst = true)]
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
                ecb.DestroyEntity(entity);
                if (!SystemAPI.HasBuffer<LeafStateElement>(request.ValueRO.Entity))
                    continue;
                var buffer = SystemAPI.GetBuffer<LeafStateElement>(request.ValueRO.Entity);
                var index = request.ValueRO.Index;
                var data = buffer[index];
                data.StateEntity = request.ValueRO.Value;
                buffer[index] = data;
            }
            ecb.Playback(state.EntityManager);
        }
    }
}