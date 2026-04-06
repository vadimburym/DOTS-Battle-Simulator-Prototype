using _Project._Code.Core.Keys;
using _Project._Code.Global.Settings;
using _Project._Code.Infrastructure;
using R3;

namespace _Project._Code.Global.SettingsService.UI.EnumSetting
{
    public sealed class EnumSettingPresenter : IEnumSettingPresenter
    {
        public ReadOnlyReactiveProperty<string> SettingName => _settingName;
        private readonly ReactiveProperty<string> _settingName = new();
        public ReadOnlyReactiveProperty<string> CurrentEnumName => _currentEnumName;
        private readonly ReactiveProperty<string> _currentEnumName = new();
        
        private readonly CompositeDisposable _compositeDisposable;
        
        private readonly SettingId _settingId;
        private readonly EnumSettingDefinition _settingDefinition;
        private readonly ISettingsService _settingsService;
        
        public EnumSettingPresenter(
            SettingId settingId,
            EnumSettingDefinition settingDefinition,
            ISettingsService settingsService)
        {
            _compositeDisposable = new();
            _settingDefinition = settingDefinition;
            _settingId = settingId;
            _settingsService = settingsService;
            
            _settingsService.Observe<int>(_settingId)
                .Subscribe(OnValueChanged).AddTo(_compositeDisposable);
            _settingName.Value = EnumUtils<SettingId>.ToString(_settingId);
        }

        private void OnValueChanged(int value)
        {
            _currentEnumName.Value = _settingDefinition.GetEnumName(value);
        }

        public void OnLeftButtonClicked()
        {
            var current = _settingsService.Get<int>(_settingId);
            var next = _settingDefinition.GetUpperValue(current);
            _settingsService.Set(_settingId, next);
        }
        public void OnRightButtonClicked()
        {
            var current = _settingsService.Get<int>(_settingId);
            var next = _settingDefinition.GetDownValue(current);
            _settingsService.Set(_settingId, next);
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}