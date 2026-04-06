using System.Collections.Generic;
using _Project._Code.Gameplay.CoreFeatures.Units.Service;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.StaticData._Root;
using _Project._Code.Infrastructure.StaticData.Units;
using _Project._Code.Locale;

namespace _Project._Code.Gameplay.CoreFeatures.Units.UI.UnitSpawnPanel
{
    public sealed class UnitSpawnPanelShower : WidgetShower<IUnitSpawnPanelPresenter, UnitSpawnPanel>
    {
        private readonly IAddressableService _addressableService;
        private readonly IUnitSpawnService _unitSpawnService;
        private readonly UnitsStaticData _unitStaticData;
        private readonly UnitSpawnPanelConfig _config;
        
        public UnitSpawnPanelShower(
            IAddressableService addressableService,
            StaticDataService staticDataService,
            IUnitSpawnService unitSpawnService,
            UnitSpawnPanelConfig config) : base(config)
        {
            _addressableService = addressableService;
            _unitSpawnService = unitSpawnService;
            _unitStaticData = staticDataService.UnitsStaticData;
            _config = config;
        }

        protected override IUnitSpawnPanelPresenter CreatePresenter()
        {
            var presenters = new List<IUnitSpawnButtonPresenter>();
            var unitsToShow = _config.UnitsToShow;
            for (int i = 0; i < unitsToShow.Length; i++)
            {
                var unitData = _addressableService.GetLoadedObject<UnitConfig>(
                    _unitStaticData.GetUnitData(unitsToShow[i]));
                presenters.Add(new UnitSpawnButtonPresenter(_unitSpawnService, unitData));
            }
            return new UnitSpawnPanelPresenter(presenters, _unitSpawnService);
        }
    }
}