using _Project._Code.Core.Contracts;
using _Project._Code.Gameplay.CoreFeatures.Entities.Systems;
using _Project._Code.Infrastructure.EcsContext;
using _Project._Code.Locale;
using VContainer;

namespace _Project._Code.GameApp.Installers
{
    public static class GameplayInstaller
    {
        public static void RegisterEcsSystems(IEcsContextBuilder builder)
        {
            builder.Register<MovementSystem>();
        }
        
        public static void Register(IContainerBuilder builder)
        {
            builder.Register<SaveLoadService>(Lifetime.Singleton).As<ISaveLoadService>();
            builder.Register<MemoryPoolService>(Lifetime.Singleton).As<IMemoryPoolService>();
            builder.Register<MemoryPoolWarmUpSystem>(Lifetime.Singleton).As<IWarmUp>();
        }
    }
}