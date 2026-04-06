using System;
using R3;

namespace _Project._Code.Global.SettingsService.UI
{
    public interface IBoolSettingPresenter : IDisposable
    {
        ReadOnlyReactiveProperty<string> SettingName { get; }
        ReadOnlyReactiveProperty<string> MarkerStatusText { get; }
        ReadOnlyReactiveProperty<bool> MarkerStatus { get; }
        void OnMarkerClicked();
    }
}