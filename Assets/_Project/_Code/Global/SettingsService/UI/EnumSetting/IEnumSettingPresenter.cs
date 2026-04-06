using System;
using R3;

namespace _Project._Code.Global.SettingsService.UI.EnumSetting
{
    public interface IEnumSettingPresenter : IDisposable
    {
        ReadOnlyReactiveProperty<string> SettingName { get; }
        ReadOnlyReactiveProperty<string> CurrentEnumName { get; }
        void OnRightButtonClicked();
        void OnLeftButtonClicked();
    }
}