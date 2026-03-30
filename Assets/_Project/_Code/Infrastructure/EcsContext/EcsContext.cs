using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using VContainer;

namespace _Project._Code.Infrastructure.EcsContext
{
    public sealed class EcsContext<TGroup> : IEcsContext
        where TGroup : ComponentSystemGroup, new()
    {
        public EntityManager EntityManager => _entityManager;
        
        private EntityManager _entityManager;
        private readonly World _world;
        private readonly ComponentSystemGroup _systemsGroup;
        private readonly List<SystemHandle> _installedSystems = new();
        private readonly List<SystemBase> _installedSystemsManaged = new();
        private readonly Dictionary<Type, EntityQuery> _entityQueries = new();
        private Action<IEcsContextBuilder> _builder;
        
        public EcsContext(Action<IEcsContextBuilder> builder = null)
        {
            _world = World.DefaultGameObjectInjectionWorld;
            _systemsGroup = _world.GetOrCreateSystemManaged<TGroup>();
            _entityManager = _world.EntityManager;
            _builder = builder;
        }

        public void WarmUpSystems(IObjectResolver diContainer) => WarmUpSystems(diContainer, _builder);
        
        public void WarmUpSystems(
            IObjectResolver diContainer,
            Action<IEcsContextBuilder> builder)
        {
            CleanUpSystems();
            var localBuilder = new EcsContextBuilder(
                diContainer,
                _world,
                _systemsGroup,
                _installedSystems,
                _installedSystemsManaged);
            builder.Invoke(localBuilder);
            _builder = builder;
            _systemsGroup.SortSystems();
            _systemsGroup.Enabled = false;
        }
        
        public void EnableSystems(bool isEnable) 
            => _systemsGroup.Enabled = isEnable;
        
        public void CleanUpSystems()
        {
            _systemsGroup.Enabled = false;
            _entityManager.CompleteAllTrackedJobs();
            for (int i = 0; i < _installedSystems.Count; i++)
            {
                var handle = _installedSystems[i];
                if (_world.Unmanaged.IsSystemValid(handle))
                    _systemsGroup.RemoveSystemFromUpdateList(handle);
            }
            for (int i = 0; i < _installedSystemsManaged.Count; i++)
                _systemsGroup.RemoveSystemFromUpdateList(_installedSystemsManaged[i]);
            
            _systemsGroup.SortSystems();
            
            for (int i = 0; i < _installedSystems.Count; i++)
            {
                var handle = _installedSystems[i];
                if (_world.Unmanaged.IsSystemValid(handle))
                    _world.DestroySystem(handle);
            }
            for (int i = 0; i < _installedSystemsManaged.Count; i++)
                _world.DestroySystemManaged(_installedSystemsManaged[i]);
            
            _installedSystems.Clear();
            _installedSystemsManaged.Clear();
        }

        public RefRW<T> GetSingletonRW<T>() where T : unmanaged, IComponentData
        {
            _entityManager.CompleteDependencyBeforeRW<T>();
            return GetOrCreateQuery<T>().GetSingletonRW<T>();
        }

        public T GetSingleton<T>() where T : unmanaged, IComponentData
        {
            return GetOrCreateQuery<T>().GetSingleton<T>();
        }

        public Entity GetSingletonEntity<T>() where T : unmanaged, IComponentData
        {
            return GetOrCreateQuery<T>().GetSingletonEntity();
        }
        
        private EntityQuery GetOrCreateQuery<T>() where T : unmanaged, IComponentData
        {
            var type = typeof(T);
            if (_entityQueries.TryGetValue(type, out var query))
                return query;
            query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<T>()
                .Build(_entityManager);
            _entityQueries.Add(type, query);
            return query;
        }
    }
    
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class LocalSystemsGroup : ComponentSystemGroup { }
    
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class BootstrapSystemsGroup : ComponentSystemGroup { }
}