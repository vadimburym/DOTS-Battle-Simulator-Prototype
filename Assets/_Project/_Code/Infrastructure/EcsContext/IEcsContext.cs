using System;
using Unity.Entities;
using VContainer;

namespace _Project._Code.Infrastructure.EcsContext
{
    public interface IEcsContext
    {
        EntityManager EntityManager { get; }
        void WarmUpSystems(IObjectResolver diContainer, Action<IEcsContextBuilder> builder);
        void EnableSystems(bool isEnable);
        void CleanUpSystems();
    }
}