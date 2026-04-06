using Unity.Entities;

namespace _Project._Code.Infrastructure.EcsContext
{
    public interface IEcsContextBuilder
    {
        IEcsContextBuilder RegisterManaged<TSystem>() where TSystem : SystemBase;
        IEcsContextBuilder Register<TSystem>() where TSystem : unmanaged, ISystem;
    }
}