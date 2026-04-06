using System;
using System.Collections.Generic;
using _Project._Code.Core.Contracts;
using _Project._Code.Core.Keys;
using _Project._Code.Global.Settings;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.EcsContext;
using _Project._Code.Infrastructure.EntitiesExtensions;
using _Project._Code.Infrastructure.ProjectContextService;
using _Project._Code.Infrastructure.StaticData._Root;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace _Project._Code.GameApp.EntryPoints
{
    public sealed class BootstrapEntryPoint : IStartable, ITickable, ILateTickable
    {
        private readonly IBootstrapContextService _bootstrapContext;
        private readonly IStateMachine _stateMachine;
        private readonly ISaveRepository _saveRepository;
        private readonly EcsContext<BootstrapSystemsGroup> _ecsBootstrap;
        private readonly IEntityPrefabService _entityPrefabService;
        private readonly ISettingsService _settingsService;
        private readonly SubSceneAwaiter _subSceneAwaiter;
        private readonly StaticDataService _staticDataService;

        public BootstrapEntryPoint(
            IBootstrapContextService bootstrapContext,
            IStateMachine stateMachine,
            ISaveRepository saveRepository,
            EcsContext<BootstrapSystemsGroup> ecsBootstrap,
            IEntityPrefabService entityPrefabService,
            ISettingsService settingsService,
            StaticDataService staticDataService,
            SubSceneAwaiter subSceneAwaiter)
        {
            _bootstrapContext = bootstrapContext;
            _stateMachine = stateMachine;
            _saveRepository = saveRepository;
            _ecsBootstrap = ecsBootstrap;
            _entityPrefabService = entityPrefabService;
            _settingsService = settingsService;
            _staticDataService = staticDataService;
            _subSceneAwaiter = subSceneAwaiter;
        }

        public void Start() => StartAsync().Forget();
        private async UniTask StartAsync()
        {
            _settingsService.WarmUp(_staticDataService.SettingsPipeline);
            _settingsService.Load();
            _ecsBootstrap.WarmUpSystems(_bootstrapContext.Container);
            _ecsBootstrap.EnableSystems(true);
            await _saveRepository.Load();
            await _subSceneAwaiter.WaitUntilSubSceneReady();
            _entityPrefabService.RebuildCache();

            _stateMachine.Enter(GameStateId.Meta);
        }

        public void Tick()
        {
            _stateMachine.Tick();
        }

        public void LateTick()
        {
            _stateMachine.LateTick();
        }
    }
}
