using System.Collections.Generic;
using _Project._Code.Gameplay.CoreFeatures.Units.Service;

namespace _Project._Code.Gameplay.CoreFeatures.Units.UI.UnitSpawnPanel
{
    public sealed class UnitSpawnPanelPresenter : IUnitSpawnPanelPresenter
    {
        public IReadOnlyList<IUnitSpawnButtonPresenter> Presenters => _presenters;
        private readonly List<IUnitSpawnButtonPresenter> _presenters;
        private readonly IUnitSpawnService _unitSpawnService;
        
        public UnitSpawnPanelPresenter(
            List<IUnitSpawnButtonPresenter> presenters,
            IUnitSpawnService unitSpawnService)
        {
            _presenters = presenters;
            _unitSpawnService = unitSpawnService;
        }
        
        public void OnClearSpawnDataClicked()
        {
            _unitSpawnService.ClearUnitSpawnData();
        }

        public void Dispose() { }
    }
}