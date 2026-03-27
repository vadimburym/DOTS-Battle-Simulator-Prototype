using _Project._Code.Gameplay.CoreFeatures.Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Systems
{
    [DisableAutoCreation]
    public partial struct EntityCleanupSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (_, entity) in SystemAPI.Query<EnabledRefRO<CleanupTag>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }
            ecb.Playback(state.EntityManager);
        }
    }
}