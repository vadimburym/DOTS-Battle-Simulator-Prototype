using _Project._Code.Gameplay.CoreFeatures.Units.Service;
using _Project._Code.Infrastructure.StaticData;
using _Project._Code.Locale;

namespace _Project._Code.Gameplay.CoreFeatures.Units.UI
{
    public class UnitCounterShower : WidgetShower<IUnitCounterPresenter, UnitCounterView>
    {
        private readonly IUnitCounterService _unitCounterService;
        
        public UnitCounterShower(
            IUnitCounterService unitCounterService,
            WidgetConfig config) : base(config)
        {
            _unitCounterService = unitCounterService;
        }

        protected override IUnitCounterPresenter CreatePresenter()
        {
            return new UnitCounterPresenter(_unitCounterService);
        }
    }
}