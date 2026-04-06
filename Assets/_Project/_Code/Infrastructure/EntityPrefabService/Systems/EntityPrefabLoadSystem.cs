using Unity.Collections;
using Unity.Entities;
using Unity.Scenes;
using VContainer;

namespace _Project._Code.Infrastructure
{
    [DisableAutoCreation]
    public partial class EntityPrefabLoadSystem : SystemBase
    {
        [Inject] private readonly IEntityPrefabService _entityPrefabService;
        
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (loading, loadingEntity) in
                     SystemAPI.Query<RefRO<EntityPrefabLoading>>().WithEntityAccess())
            {
                var loadHandleEntity = loading.ValueRO.LoadHandleEntity;
                if (!EntityManager.Exists(loadHandleEntity))
                    continue;
                if (!EntityManager.HasComponent<PrefabRoot>(loadHandleEntity))
                    continue;

                var result = EntityManager.GetComponentData<PrefabRoot>(loadHandleEntity);

                _entityPrefabService.OnPrefabLoaded(
                    loading.ValueRO.EntityPoolId,
                    result.Root,
                    loadHandleEntity);

                ecb.DestroyEntity(loadingEntity);
            }
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}