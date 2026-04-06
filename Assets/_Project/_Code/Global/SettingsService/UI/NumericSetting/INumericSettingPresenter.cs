using System;
using R3;

namespace _Project._Code.Global.SettingsService.UI.Settings
{
    public interface INumericSettingPresenter : IDisposable
    {
        ReadOnlyReactiveProperty<string> SettingName { get; }
        ReadOnlyReactiveProperty<float> FloatValue { get; }
        float MinValue { get; }
        float MaxValue { get; }
        bool WholeNumbers { get; }
        void OnValueChangedFromSlider(float value);
    }
}