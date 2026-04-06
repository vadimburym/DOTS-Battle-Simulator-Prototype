using VContainer;
using VContainer.Unity;

namespace _Project._Code.Infrastructure.ProjectContextService
{
    public sealed class BootstrapContextService : IBootstrapContextService
    {
        public LifetimeScope Context => _context;
        public IObjectResolver Container => _container;

        private readonly LifetimeScope _context;
        private readonly IObjectResolver _container;

        public BootstrapContextService(LifetimeScope context)
        {
            _context = context;
            _container = context.Container;
        }
    }
}
