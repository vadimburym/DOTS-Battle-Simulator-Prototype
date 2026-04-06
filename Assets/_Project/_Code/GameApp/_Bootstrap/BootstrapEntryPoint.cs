using _Project._Code.Core.Keys;
using _Project._Code.Global.Settings;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.EcsContext;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace _Project._Code.GameApp.EntryPoints
{
    public sealed class BootstrapEntryPoint : IStartable, ITickable, ILateTickable
    {
        private readonly IStateMachine _stateMachine;
        private readonly ISaveRepository _saveRepository;
        private readonly EcsContext<BootstrapSystemsGroup> _ecsBootstrap;
        private readonly IEntityPrefabService _entityPrefabService;
        private readonly ISettingsService _settingsService;
        
        public BootstrapEntryPoint(
            IStateMachine stateMachine,
            ISaveRepository saveRepository,
            EcsContext<BootstrapSystemsGroup> ecsBootstrap,
            IEntityPrefabService entityPrefabService,
            ISettingsService settingsService)
        {
            _stateMachine = stateMachine;
            _saveRepository = saveRepository;
            _ecsBootstrap = ecsBootstrap;
            _entityPrefabService = entityPrefabService;
            _settingsService = settingsService;
        }

        public void Start() => StartAsync().Forget();
        private async UniTask StartAsync()
        {
            _settingsService.Load();
            _ecsBootstrap.WarmUpSystems(BootstrapContext.Instance.Container);
            _ecsBootstrap.EnableSystems(true);
            await _saveRepository.Load();
            await BootstrapContext.Instance.BootstrapSubSceneAwaiter.WaitUntilSubSceneReady();
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