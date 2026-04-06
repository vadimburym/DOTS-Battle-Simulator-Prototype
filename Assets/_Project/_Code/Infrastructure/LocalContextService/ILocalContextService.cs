using System;
using VContainer;
using VContainer.Unity;

namespace _Project._Code.Infrastructure
{
    public interface ILocalContextService
    {
        IObjectResolver Container { get; }
        void CleanUp();
        void WarmUp(LifetimeScope parent, Action<IContainerBuilder> installation, string contextName = null);
    }
}