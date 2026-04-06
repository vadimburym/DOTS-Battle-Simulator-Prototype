using _Project._Code.Core.Keys;
using Cysharp.Threading.Tasks;
using Unity.Entities;

namespace _Project._Code.Infrastructure
{
    public interface IEntityPrefabService
    {
        void RebuildCache();
        UniTask LoadAsync(EntityPoolId[] entityIds);
        UniTask<Entity> LoadAsync(EntityPoolId entityId);
        void Unload(EntityPoolId entityId);
        Entity GetEntityPrefab(EntityPoolId entityId);
        bool TryGetEntityPrefab(EntityPoolId entityId, out Entity prefabEntity);
        void OnPrefabLoaded(EntityPoolId entityId, Entity prefab, Entity loadHandleEntity);
    }
}