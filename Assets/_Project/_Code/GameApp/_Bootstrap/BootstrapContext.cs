using _Project._Code.Core.Abstractions;
using _Project._Code.GameApp.EntryPoints;
using _Project._Code.GameApp.GameStates;
using _Project._Code.GameApp.SaveStrategies;
using _Project._Code.Global.Settings;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.ApplicationService;
using _Project._Code.Infrastructure.Audio;
using _Project._Code.Infrastructure.EcsContext;
using _Project._Code.Infrastructure.EntitiesExtensions;
using _Project._Code.Infrastructure.LoadingCurtainProvider;
using _Project._Code.Infrastructure.ProjectContextService;
using _Project._Code.Infrastructure.StaticData._Root;
using _Project._Code.Tools.FrameDebug;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Project._Code.GameApp
{
    public sealed class BootstrapContext : LifetimeScope
    {
        [SerializeField] private StaticDataService _staticDataService;
        [SerializeField] private UIInstallerService _uiInstallerService;
        [SerializeField] private SubSceneAwaiter _bootstrapSubSceneAwaiter;
        [SerializeField] private LoadingCurtainProvider _loadingCurtainProvider;
        [SerializeField] private AudioProvider _audioProvider;
        [SerializeField] private FrameDebugProvider _frameDebugProvider;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterInfrastructure(builder);
            RegisterGameStates(builder);
            RegisterEcsBootstrap(builder);
            builder.Register<AppDataSaveStrategy>(Lifetime.Singleton).As<ISaveStrategy>();

            builder.RegisterEntryPoint<BootstrapEntryPoint>().WithParameter(_bootstrapSubSceneAwaiter);
        }

        private void RegisterGameStates(IContainerBuilder builder)
        {
            builder.Register<MetaState>(Lifetime.Singleton).As<IGameState>();
            builder.Register<GameplayState>(Lifetime.Singleton).As<IGameState>();
        }

        private void RegisterInfrastructure(IContainerBuilder builder)
        {
            builder.Register<BootstrapContextService>(Lifetime.Singleton).As<IBootstrapContextService>().WithParameter(this);
            builder.RegisterInstance(_staticDataService).As<StaticDataService>();
            builder.RegisterInstance(_uiInstallerService).As<UIInstallerService>();
            builder.RegisterInstance(_loadingCurtainProvider).As<ILoadingCurtainProvider>();
            builder.RegisterInstance(_audioProvider).As<IAudioProvider>();
            builder.RegisterInstance(_frameDebugProvider).As<IFrameDebugProvider>();
            builder.Register<SettingsService>(Lifetime.Singleton).As<ISettingsService>();
            builder.Register<StateMachine>(Lifetime.Singleton).As<IStateMachine>();
            builder.Register<ApplicationService>(Lifetime.Singleton).As<IApplicationService>();
            builder.Register<SaveRepository>(Lifetime.Singleton).As<ISaveRepository>();
            builder.Register<LocalContextService>(Lifetime.Singleton).As<ILocalContextService>();
            builder.Register<GamePauseService>(Lifetime.Singleton).As<IGamePauseService>();
            builder.Register<SceneLoadService>(Lifetime.Singleton).As<ISceneLoadService>();
            builder.Register<AddressableService>(Lifetime.Singleton).As<IAddressableService>();
            builder.Register<EntityPrefabService>(Lifetime.Singleton).As<IEntityPrefabService>();
            builder.Register<InputService>(Lifetime.Singleton).As<IInputService>();
            builder.Register<MainCameraService>(Lifetime.Singleton).As<IMainCameraService>();
            builder.RegisterInstance(new EcsContext<LocalSystemsGroup>()).As<IEcsContext>();
        }

        private void RegisterEcsBootstrap(IContainerBuilder builder)
        {
            builder.RegisterInstance(new EcsContext<BootstrapSystemsGroup>(ecsBuilder =>
            {
                ecsBuilder.RegisterManaged<EntityPrefabLoadSystem>();
                ecsBuilder.RegisterManaged<FrameDebugSystem>();
                ecsBuilder.RegisterManaged<FrameDebugSettingSyncSystem>();
                ecsBuilder.RegisterManaged<ApplicationSettingsSyncSystem>();
                ecsBuilder.RegisterManaged<AudioSettingsSyncSystem>();
            }))
                .As<EcsContext<BootstrapSystemsGroup>>();
        }
    }
}
