using _Project._Code.Core.Abstractions;
using _Project._Code.Core.Keys;
using _Project._Code.GameApp.EntryPoints;
using _Project._Code.Infrastructure;
using Cysharp.Threading.Tasks;
using VContainer;

namespace _Project._Code.GameApp.GameStates
{
    public sealed class MetaState : IGameState
    {
        GameStateId IGameState.GameStateId => GameStateId.Meta;
        
        private readonly ILocalContextService _localContextService;
        
        public MetaState(
            ILocalContextService localContextService)
        {
            _localContextService = localContextService;
        }
        
        public void Enter() => EnterAsync().Forget();
        private async UniTask EnterAsync()
        {
            await UniTask.Delay(1000);
            
            _localContextService.WarmUp(BootstrapContext.Instance,builder =>
            {
                builder.Register<MetaEntryPoint>(Lifetime.Scoped);
            });
        }
        
        public void Tick()
        {
            
        }

        public void LateTick()
        {
            
        }
        
        public void Dispose()
        {
            _localContextService.CleanUp();
        }
    }
}