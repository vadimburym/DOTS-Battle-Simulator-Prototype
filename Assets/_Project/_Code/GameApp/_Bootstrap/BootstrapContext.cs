using _Project._Code.Core.Abstractions;
using _Project._Code.GameApp.EntryPoints;
using _Project._Code.GameApp.GameStates;
using _Project._Code.GameApp.SaveStrategies;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.EcsContext;
using _Project._Code.Infrastructure.EntitiesExtensions;
using _Project._Code.Infrastructure.StaticData._Root;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project._Code.GameApp
{
    public sealed class BootstrapContext : LifetimeScope
    {
        public StaticDataService StaticDataService;
        public UIInstallerService UIInstallerService;
        public SubSceneAwaiter BootstrapSubSceneAwaiter;
        public GameObject CameraGameObject;
        
        public static BootstrapContext Instance;
        
        protected override void Awake()
        {
            if (Instance != null)
                Destroy(gameObject);
            base.Awake();
            Instance = this;
        }
        
        protected override void Configure(IContainerBuilder builder)
        {
            RegisterInfrastructure(builder);
            RegisterGameStates(builder);
            RegisterEcsBootstrap(builder);
            builder.Register<AppDataSaveStrategy>(Lifetime.Singleton).As<ISaveStrategy>();
            
            builder.RegisterEntryPoint<BootstrapEntryPoint>();
        }
        
        private void RegisterGameStates(IContainerBuilder builder)
        {
            builder.Register<MetaState>(Lifetime.Singleton).As<IGameState>();
            builder.Register<GameplayState>(Lifetime.Singleton).As<IGameState>();
        }

        private void RegisterInfrastructure(IContainerBuilder builder)
        {
            builder.RegisterInstance(StaticDataService).As<StaticDataService>();
            builder.RegisterInstance(UIInstallerService).As<UIInstallerService>();
            builder.Register<StateMachine>(Lifetime.Singleton).As<IStateMachine>();
            builder.Register<SaveRepository>(Lifetime.Singleton).As<ISaveRepository>();
            builder.Register<LocalContextService>(Lifetime.Singleton).As<ILocalContextService>();
            builder.Register<GamePauseService>(Lifetime.Singleton).As<IGamePauseService>();
            builder.Register<SceneLoadService>(Lifetime.Singleton).As<ISceneLoadService>();
            builder.Register<AddressableService>(Lifetime.Singleton).As<IAddressableService>();
            builder.Register<EntityPrefabService>(Lifetime.Singleton).As<IEntityPrefabService>();
            builder.Register<InputService>(Lifetime.Singleton).As<IInputService>();
            builder.RegisterInstance(new EcsContext<LocalSystemsGroup>()).As<IEcsContext>();
        }
        
        private void RegisterEcsBootstrap(IContainerBuilder builder)
        {
            builder.RegisterInstance(new EcsContext<BootstrapSystemsGroup>(ecsBuilder =>
            {
                ecsBuilder.RegisterManaged<EntityPrefabLoadSystem>();
            }))
                .As<EcsContext<BootstrapSystemsGroup>>();
        }
    }
}