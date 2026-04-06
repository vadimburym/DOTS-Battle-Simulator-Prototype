using System.Collections.Generic;
using _Project._Code.Core.Keys;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Scenes;
using UnityEngine;

namespace _Project._Code.Infrastructure
{
    public sealed class EntityPrefabService : IEntityPrefabService
    {
        private readonly World _world;
        private EntityManager _entityManager;
        private EntityQuery _registryQuery;
        
        private readonly Dictionary<EntityPoolId, EntityPrefabReference> _allReferences = new();
        private readonly Dictionary<EntityPoolId, Entity> _loadedPrefabs = new();
        private readonly Dictionary<EntityPoolId, UniTaskCompletionSource<Entity>> _loadingTasks = new();
        private readonly List<UniTask> _taskCache = new();
        
        public EntityPrefabService()
        {
            _world = World.DefaultGameObjectInjectionWorld;
            _entityManager = _world.EntityManager;

            _registryQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<EntityPrefabSingleton>()
                .WithAll<EntityPrefabReferenceElement>()
                .WithAll<EntityPrefabElement>()
                .Build(_entityManager);

            RebuildCache();
        }

        public void RebuildCache()
        {
            _allReferences.Clear();
            if (_registryQuery.IsEmptyIgnoreFilter)
                return;
            var referenceBuffer = _registryQuery.GetSingletonBuffer<EntityPrefabReferenceElement>(true);
            for (int i = 0; i < referenceBuffer.Length; i++)
                _allReferences[referenceBuffer[i].EntityPoolId] = referenceBuffer[i].PrefabReference;
            
            _loadedPrefabs.Clear();
            if (_registryQuery.IsEmptyIgnoreFilter)
                return;
            var loadedBuffer = _registryQuery.GetSingletonBuffer<EntityPrefabElement>(true);
            for (int i = 0; i < loadedBuffer.Length; i++)
                _loadedPrefabs[loadedBuffer[i].EntityPoolId] = loadedBuffer[i].Prefab;
        }
        
        public bool TryGetEntityPrefab(EntityPoolId entityId, out Entity prefab)
            => _loadedPrefabs.TryGetValue(entityId, out prefab);

        public Entity GetEntityPrefab(EntityPoolId entityId)
            => _loadedPrefabs[entityId];
        
        public async UniTask LoadAsync(EntityPoolId[] entityIds)
        {
            _taskCache.Clear();
            for (int i = 0; i < entityIds.Length; i++)
                _taskCache.Add(LoadAsync(entityIds[i]));
            await UniTask.WhenAll(_taskCache);
        }
        
        public async UniTask<Entity> LoadAsync(EntityPoolId entityId)
        {
            if (_loadedPrefabs.TryGetValue(entityId, out var loaded))
                return loaded;
            if (_loadingTasks.TryGetValue(entityId, out var existingTask))
                return await existingTask.Task;
            if (!_allReferences.TryGetValue(entityId, out var prefabReference))
                throw new System.Exception($"Prefab reference for EntityId {entityId} not found.");

            var tcs = new UniTaskCompletionSource<Entity>();
            _loadingTasks.Add(entityId, tcs);
            
            var loadHandle = SceneSystem.LoadPrefabAsync(_world.Unmanaged, prefabReference);
            var loadingEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(loadingEntity, new EntityPrefabLoading
            {
                EntityPoolId = entityId,
                LoadHandleEntity = loadHandle
            });

            return await tcs.Task;
        }

        public void Unload(EntityPoolId[] entityId)
        {
            for (int i = 0; i < entityId.Length; i++)
                Unload(entityId[i]);
        }
        
        public void Unload(EntityPoolId entityId)
        {
            _loadedPrefabs.Remove(entityId);
            
            if (!_registryQuery.IsEmptyIgnoreFilter)
            {
                var loadedBuffer = _registryQuery.GetSingletonBuffer<EntityPrefabElement>();
                int index = -1;
                var loadHandleEntity = Entity.Null;
                for (int i = 0; i < loadedBuffer.Length; i++)
                {
                    if (loadedBuffer[i].EntityPoolId != entityId)
                        continue;
                    index = i;
                    loadHandleEntity = loadedBuffer[i].LoadHandleEntity;
                    break;
                }

                if (index >= 0)
                {
                    loadedBuffer.RemoveAt(index);
                    SceneSystem.UnloadScene(
                        _world.Unmanaged,
                        loadHandleEntity,
                        SceneSystem.UnloadParameters.Default);
                }
            }
            
            if (_loadingTasks.Remove(entityId, out var tcs))
                tcs.TrySetCanceled();
        }
        
        public void OnPrefabLoaded(EntityPoolId entityId, Entity prefab, Entity loadHandleEntity)
        {
            _loadedPrefabs[entityId] = prefab;

            if (!_registryQuery.IsEmptyIgnoreFilter)
            {
                var loadedBuffer = _registryQuery.GetSingletonBuffer<EntityPrefabElement>();
                bool exists = false;
                for (int i = 0; i < loadedBuffer.Length; i++)
                {
                    if (loadedBuffer[i].EntityPoolId == entityId)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    loadedBuffer.Add(new EntityPrefabElement
                    {
                        EntityPoolId = entityId,
                        Prefab = prefab,
                        LoadHandleEntity = loadHandleEntity
                    });
                }
            }

            if (_loadingTasks.Remove(entityId, out var tcs))
                tcs.TrySetResult(prefab);
        }
    }
}