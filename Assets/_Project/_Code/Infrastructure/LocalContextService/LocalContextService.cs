using System;
using VContainer;
using VContainer.Unity;

namespace _Project._Code.Infrastructure
{
    public sealed class LocalContextService : ILocalContextService
    {
        public IObjectResolver Container => _context.Container;
        private LifetimeScope _context;
        
        public void CleanUp() => _context?.Dispose();

        public void WarmUp(LifetimeScope parent, Action<IContainerBuilder> installation, string contextName = null)
        {
            _context?.Dispose();
            _context = parent.CreateChild(installation, contextName);
        }
    }
}