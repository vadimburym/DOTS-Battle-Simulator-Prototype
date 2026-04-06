using _Project._Code.Core.Keys;
using _Project._Code.Global.Settings;
using _Project._Code.Infrastructure;
using R3;

namespace _Project._Code.Global.SettingsService.UI
{
    public sealed class BoolSettingPresenter : IBoolSettingPresenter
    {
        public ReadOnlyReactiveProperty<string> SettingName => _settingName;
        private readonly ReactiveProperty<string> _settingName = new();
        public ReadOnlyReactiveProperty<string> MarkerStatusText => _markerStatusText;
        private readonly ReactiveProperty<string> _markerStatusText = new();
        public ReadOnlyReactiveProperty<bool> MarkerStatus => _markerStatus;
        private readonly ReactiveProperty<bool> _markerStatus = new();
        
        private readonly SettingId _settingId;
        private readonly ISettingsService _settingsService;
        private readonly CompositeDisposable _compositeDisposable = new();

        public BoolSettingPresenter(
            SettingId settingId,
            ISettingsService settingsService)
        {
            _settingId = settingId;
            _settingsService = settingsService;
            _settingsService.Observe<bool>(_settingId)
                .Subscribe(OnValueChanged).AddTo(_compositeDisposable);
            _settingName.Value = EnumUtils<SettingId>.ToString(_settingId);
        }
        
        private void OnValueChanged(bool value)
        {
            _markerStatus.Value = value;
            _markerStatusText.Value = value ? "enabled" : "disabled";
        }

        public void OnMarkerClicked()
        {
            var value = _settingsService.Get<bool>(_settingId);
            _settingsService.Set(_settingId, !value);
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}