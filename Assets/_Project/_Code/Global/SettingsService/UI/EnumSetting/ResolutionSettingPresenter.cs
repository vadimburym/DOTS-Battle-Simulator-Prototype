using _Project._Code.Core.Keys;
using _Project._Code.Global.Settings;
using _Project._Code.Global.SettingsService.UI.EnumSetting;
using _Project._Code.Infrastructure;
using _Project._Code.Infrastructure.ApplicationService;
using R3;

namespace _Project._Code.Global.SettingsService.UI
{
    public class ResolutionSettingPresenter : IEnumSettingPresenter
    {
        public ReadOnlyReactiveProperty<string> SettingName => _settingName;
        private readonly ReactiveProperty<string> _settingName = new();
        public ReadOnlyReactiveProperty<string> CurrentEnumName => _currentEnumName;
        private readonly ReactiveProperty<string> _currentEnumName = new();
        
        private readonly CompositeDisposable _compositeDisposable;
        
        private readonly SettingId _settingId;
        private readonly IApplicationService _applicationService;
        private readonly ISettingsService _settingsService;
        
        public ResolutionSettingPresenter(
            SettingId settingId,
            IApplicationService applicationService,
            ISettingsService settingsService)
        {
            _compositeDisposable = new();
            _applicationService = applicationService;
            _settingId = settingId;
            _settingsService = settingsService;
            
            _settingsService.Observe<float>(_settingId)
                .Subscribe(OnValueChanged).AddTo(_compositeDisposable);
            _settingName.Value = EnumUtils<SettingId>.ToString(_settingId);
        }

        private void OnValueChanged(float value)
        {
            _currentEnumName.Value = _applicationService.GetResolutionOption((int)value).ToString();
        }

        public void OnLeftButtonClicked()
        {
            var current = (int)_settingsService.Get<float>(_settingId);
            var next = current - 1;
            if (next < 0) next = _applicationService.AvailableResolutions.Count - 1;
            _settingsService.Set(_settingId, (float)next);
        }
        
        public void OnRightButtonClicked()
        {
            var current = (int)_settingsService.Get<float>(_settingId);
            var next = current + 1;
            if (next >= _applicationService.AvailableResolutions.Count) next = 0;
            _settingsService.Set(_settingId, (float)next);
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}