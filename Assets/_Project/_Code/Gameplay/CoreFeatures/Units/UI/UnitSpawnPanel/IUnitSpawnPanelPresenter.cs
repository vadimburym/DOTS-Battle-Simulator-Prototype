using System.Collections.Generic;
using _Project._Code.Locale;

namespace _Project._Code.Gameplay.CoreFeatures.Units.UI.UnitSpawnPanel
{
    public interface IUnitSpawnPanelPresenter : IWidgetPresenter
    {
        void OnClearSpawnDataClicked();
        IReadOnlyList<IUnitSpawnButtonPresenter> Presenters { get; }
    }
}