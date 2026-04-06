using _Project._Code.Core.Keys;
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
        
        public BootstrapEntryPoint(
            IStateMachine stateMachine,
            ISaveRepository saveRepository,
            EcsContext<BootstrapSystemsGroup> ecsBootstrap,
            IEntityPrefabService entityPrefabService)
        {
            _stateMachine = stateMachine;
            _saveRepository = saveRepository;
            _ecsBootstrap = ecsBootstrap;
            _entityPrefabService = entityPrefabService;
        }

        public void Start() => StartAsync().Forget();
        private async UniTask StartAsync()
        {
            await _saveRepository.Load();
            await BootstrapContext.Instance.BootstrapSubSceneAwaiter.WaitUntilSubSceneReady();
            _ecsBootstrap.WarmUpSystems(BootstrapContext.Instance.Container);
            _ecsBootstrap.EnableSystems(true);
            _entityPrefabService.RebuildCache();
            
            _stateMachine.Enter(GameStateId.Gameplay);
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