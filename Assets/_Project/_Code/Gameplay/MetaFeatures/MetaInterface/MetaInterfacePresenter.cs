using _Project._Code.Core.Keys;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.ApplicationService;

namespace _Project._Code.Gameplay.MetaFeatures.MetaInterface
{
    public sealed class MetaInterfacePresenter : IMetaInterfacePresenter
    {
        private readonly IApplicationService _applicationService;
        private readonly IStateMachine _stateMachine;
        
        public MetaInterfacePresenter(
            IApplicationService applicationService,
            IStateMachine stateMachine)
        {
            _applicationService = applicationService;
            _stateMachine = stateMachine;
        }
        
        public void OnPlayClicked() => _stateMachine.Enter(GameStateId.Gameplay);
        public void OnExitClicked() => _applicationService.Quit();
        public void Dispose() { }
    }
}