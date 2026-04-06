using System.Collections.Generic;
using Unity.Entities;
using VContainer;

namespace _Project._Code.Infrastructure.EcsContext
{
    public sealed class EcsContextBuilder : IEcsContextBuilder
    {
        private readonly World _world;
        private readonly ComponentSystemGroup _group;
        private readonly List<SystemHandle> _installedSystems;
        private readonly List<SystemBase> _installedSystemsManaged;
        private readonly IObjectResolver _diContainer;
        
        public EcsContextBuilder(
            IObjectResolver diContainer,
            World world,
            ComponentSystemGroup group,
            List<SystemHandle> installedSystems,
            List<SystemBase> installedSystemsManaged)
        {
            _world = world;
            _group = group;
            _installedSystems = installedSystems;
            _installedSystemsManaged = installedSystemsManaged;
            _diContainer = diContainer;
        }

        public IEcsContextBuilder RegisterManaged<TSystem>() where TSystem : SystemBase
        {
            var system = _world.GetOrCreateSystemManaged<TSystem>();
            if (!_installedSystemsManaged.Contains(system))
            {
                _group.AddSystemToUpdateList(system);
                _installedSystemsManaged.Add(system);
                _diContainer.Inject(system);
            }
            return this;
        }
        
        public IEcsContextBuilder Register<TSystem>() where TSystem : unmanaged, ISystem
        {
            var handle = _world.GetOrCreateSystem<TSystem>();
            if (!_installedSystems.Contains(handle))
            {
                _group.AddSystemToUpdateList(handle);
                _installedSystems.Add(handle);
            }
            return this;
        }
    }
}