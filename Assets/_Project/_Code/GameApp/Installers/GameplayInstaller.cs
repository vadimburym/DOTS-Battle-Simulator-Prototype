using _Project._Code.Core.Contracts;
using _Project._Code.Gameplay.CoreFeatures;
using _Project._Code.Gameplay.CoreFeatures.AI._Root;
using _Project._Code.Gameplay.CoreFeatures.Entities.AiSystems;
using _Project._Code.Gameplay.CoreFeatures.Entities.Systems;
using _Project._Code.Gameplay.CoreFeatures.EyeSensorGrid.Systems;
using _Project._Code.Gameplay.CoreFeatures.Units.Factory;
using _Project._Code.Gameplay.CoreFeatures.Units.Service;
using _Project._Code.Gameplay.CoreFeatures.Units.Systems;
using _Project._Code.Infrastructure.EcsContext;
using _Project._Code.Locale;
using _Project._Code.Locale.EdgeScrollCamera;
using VContainer;

namespace _Project._Code.GameApp.Installers
{
    public static class GameplayInstaller
    {
        public static void RegisterEcsSystems(IEcsContextBuilder builder)
        {
            //---Entities
            builder.Register<MovementSystem>();
            builder.Register<MovementAnimationSystem>();
            builder.Register<MoveCommandSystem>();
            builder.Register<SelectedViewSystem>();
            builder.Register<TakeDamageSystem>();
            builder.RegisterManaged<SelectionSystem>();
            builder.RegisterManaged<ClickToMoveSelectedSystem>();
            builder.Register<EntityCleanupSystem>();
            builder.Register<CorpseSystem>();
            //---BattlefieldGrid
            builder.Register<GridMovingCellSyncSystem>();
            builder.Register<GridOccupiedCleanupSystem>();
            //---EyeSensor
            builder.Register<EyeSensorGridSystem>();
            builder.Register<EyeSensorSystem>();
            builder.Register<SeeToDetectedSystem>();
            //---AICore
            builder.RegisterManaged<BehaviourTreeInitSystem>();
            builder.Register<LeafStateWriteSystem>();
            builder.Register<BehaviourTreeTickSystem>();
            //---AIStates
            builder.Register<AttackStateSystem>();
            builder.Register<ChaseStateSystem>();
            //---Units
            builder.RegisterManaged<UnitFabricateSystem>();
            builder.RegisterManaged<UnitCleanupSystem>();
            builder.RegisterManaged<UnitSpawnSystem>();
        }
        
        public static void Register(IContainerBuilder builder)
        {
            RegisterLocal(builder);
            RegisterUnits(builder);
        }

        private static void RegisterLocal(IContainerBuilder builder)
        {
            builder.Register<SaveLoadService>(Lifetime.Singleton).As<ISaveLoadService>();
            builder.Register<WidgetService>(Lifetime.Singleton).As<IWidgetService>();
            builder.Register<MemoryPoolService>(Lifetime.Singleton).As<IMemoryPoolService>();
            builder.Register<MemoryPoolWarmUpSystem>(Lifetime.Singleton).As<IWarmUp>();
            builder.Register<EdgeScrollCameraSystem>(Lifetime.Singleton).As<ILateTick>();
        }
        
        private static void RegisterUnits(IContainerBuilder builder)
        {
            builder.Register<UnitFactory>(Lifetime.Singleton).As<IUnitFactory>();
            builder.Register<UnitsInitSystem>(Lifetime.Singleton).As<IInit>();
            builder.Register<UnitCounterService>(Lifetime.Singleton).As<IUnitCounterService>();
            builder.Register<UnitSpawnService>(Lifetime.Singleton).As<IUnitSpawnService>();
        }
    }
}