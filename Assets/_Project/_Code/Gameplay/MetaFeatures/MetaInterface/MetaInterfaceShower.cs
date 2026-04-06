using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.ApplicationService;
using _Project._Code.Infrastructure.StaticData;
using _Project._Code.Locale;

namespace _Project._Code.Gameplay.MetaFeatures.MetaInterface
{
    public class MetaInterfaceShower : WidgetShower<IMetaInterfacePresenter, MetaInterfaceWidget>
    {
        private readonly IApplicationService _applicationService;
        private readonly IStateMachine _stateMachine;
        
        public MetaInterfaceShower(
            IApplicationService applicationService,
            IStateMachine stateMachine,
            WidgetConfig config) : base(config)
        {
            _applicationService = applicationService;
            _stateMachine = stateMachine;
        }

        protected override IMetaInterfacePresenter CreatePresenter()
        {
            return new MetaInterfacePresenter(_applicationService, _stateMachine);
        }
    }
}