using VContainer;
using VContainer.Unity;

namespace _Project._Code.Infrastructure.ProjectContextService
{
    public interface IBootstrapContextService
    {
        public LifetimeScope Context { get; }
        public IObjectResolver Container { get; }
    }
}
