using _Project._Code.Core.Contracts;
using _Project._Code.Gameplay.CoreFeatures;
using _Project._Code.Gameplay.CoreFeatures.Entities.Systems;
using _Project._Code.Gameplay.CoreFeatures.Units.Factory;
using _Project._Code.Gameplay.CoreFeatures.Units.Systems;
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
            builder.RegisterManaged<ClickToMoveSelectedSystem>();
            builder.Register<SelectedViewSystem>();
            builder.Register<MoveCommandSystem>();
            builder.Register<GridRuntimeMapSystem>();
            builder.Register<GridOccupancySyncSystem>();
            builder.RegisterManaged<SelectionSystem>();
        }
        
        public static void Register(IContainerBuilder builder)
        {
            RegisterLocal(builder);
            RegisterUnits(builder);
        }

        private static void RegisterLocal(IContainerBuilder builder)
        {
            builder.Register<SaveLoadService>(Lifetime.Singleton).As<ISaveLoadService>();
            builder.Register<MemoryPoolService>(Lifetime.Singleton).As<IMemoryPoolService>();
            builder.Register<MemoryPoolWarmUpSystem>(Lifetime.Singleton).As<IWarmUp>();
        }
        
        private static void RegisterUnits(IContainerBuilder builder)
        {
            builder.Register<UnitFactory>(Lifetime.Singleton).As<IUnitFactory>();
            builder.Register<UnitsInitSystem>(Lifetime.Singleton).As<IInit>();
        }
    }
}