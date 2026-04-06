using _Project._Code.Core.Keys;
using _Project._Code.Global.Settings;
using _Project._Code.Infrastructure;
using R3;

namespace _Project._Code.Global.SettingsService.UI.Settings
{
    public sealed class NumericSettingPresenter : INumericSettingPresenter
    {
        public ReadOnlyReactiveProperty<string> SettingName => _settingName;
        private readonly ReactiveProperty<string> _settingName = new();
        public ReadOnlyReactiveProperty<float> FloatValue => _floatValue;
        private readonly ReactiveProperty<float> _floatValue = new();
        public float MinValue => _settingData.Min;
        public float MaxValue => _settingData.Max;
        public bool WholeNumbers => _settingData.WholeNumbers;

        private readonly SettingId _settingId;
        private readonly NumericSettingDefinition _settingData;
        private readonly ISettingsService _settingsService;
        private readonly CompositeDisposable _compositeDisposable;

        public NumericSettingPresenter(
            SettingId settingId,
            NumericSettingDefinition settingData,
            ISettingsService settingsService)
        {
            _settingId = settingId;
            _settingData = settingData;
            _settingsService = settingsService;
            _compositeDisposable = new();
            
            _settingName.Value = EnumUtils<SettingId>.ToString(_settingId);
            _settingsService.Observe<float>(_settingId)
                .Subscribe(value => _floatValue.Value = value).AddTo(_compositeDisposable);
        }
        
        public void OnValueChangedFromSlider(float value)
        {
            _settingsService.Set<float>(_settingId, value);
        }
        
        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}