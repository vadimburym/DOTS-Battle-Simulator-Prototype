using _Project._Code.Core.Contracts;
using _Project._Code.Infrastructure.EcsContext;
using _Project._Code.Locale;
using VContainer;

namespace _Project._Code.GameApp.Installers
{
    public static class MetaInstaller
    {
        public static void RegisterEcsSystems(IEcsContextBuilder builder)
        {

        }
        
        public static void Register(IContainerBuilder builder)
        {
            RegisterLocal(builder);
        }
        
        private static void RegisterLocal(IContainerBuilder builder)
        {
            builder.Register<SaveLoadService>(Lifetime.Singleton).As<ISaveLoadService>();
            builder.Register<WidgetService>(Lifetime.Singleton).As<IWidgetService>();
            builder.Register<MemoryPoolService>(Lifetime.Singleton).As<IMemoryPoolService>();
            builder.Register<MemoryPoolWarmUpSystem>(Lifetime.Singleton).As<IWarmUp>();
            builder.Register<SceneEntryCameraSystem>(Lifetime.Singleton).As<IInit>();
        }
    }
}