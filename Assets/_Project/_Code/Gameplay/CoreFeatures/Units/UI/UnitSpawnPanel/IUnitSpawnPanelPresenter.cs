using System;
using System.Collections.Generic;

namespace _Project._Code.Gameplay.CoreFeatures.Units.UI.UnitSpawnPanel
{
    public interface IUnitSpawnPanelPresenter : IDisposable
    {
        void OnClearSpawnDataClicked();
        IReadOnlyList<IUnitSpawnButtonPresenter> Presenters { get; }
    }
}