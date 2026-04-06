using System;
using System.Collections.Generic;

namespace _Project._Code.Global.SettingsService.UI
{
    public interface ISettingsPresenter : IDisposable
    {
        IReadOnlyList<SettingGroup> Groups { get; }
        void OnConfirmButtonClicked();
        void OnQuitButtonClicked();
    }
}