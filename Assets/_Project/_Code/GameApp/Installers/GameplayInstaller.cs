using _Project._Code.Core.Contracts;
using _Project._Code.Gameplay.CoreFeatures;
using _Project._Code.Gameplay.CoreFeatures.Entities.AiSystems;
using _Project._Code.Gameplay.CoreFeatures.Entities.Systems;
using _Project._Code.Gameplay.CoreFeatures.EyeSensorGrid.Systems;
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
            //---Entities
            builder.Register<MovementSystem>();
            builder.Register<MoveCommandSystem>();
            builder.Register<SelectedViewSystem>();
            builder.RegisterManaged<SelectionSystem>();
            builder.RegisterManaged<ClickToMoveSelectedSystem>();
            //---BattlefieldGrid
            builder.Register<GridMovingCellSyncSystem>();
            //---AiStates
            builder.Register<AttackStateSystem>();
            builder.Register<ChaseStateSystem>();
            //---EyeSensor
            builder.Register<EyeSensorGridSystem>();
            builder.Register<EyeSensorSystem>();
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