using _Project._Code.Core.Keys;
using _Project._Code.Infrastructure.ApplicationService;
using R3;
using Unity.Entities;
using UnityEngine;
using VContainer;

namespace _Project._Code.Global.Settings
{
    [DisableAutoCreation]
    public partial class ApplicationSettingsSyncSystem : SystemBase
    {
        private IApplicationService _applicationService;
        private ISettingsService _settingsService;
        private CompositeDisposable _disposable;
        
        [Inject]
        private void Construct(
            IApplicationService applicationService,
            ISettingsService settingsService)
        {
            _disposable = new CompositeDisposable();
            _applicationService = applicationService;
            _settingsService = settingsService;
            _settingsService.ObserveEnum<FullScreenMode>(SettingId.FullScreenMode)
                .Subscribe(OnFullScreenModeSetting).AddTo(_disposable);
            _settingsService.Observe<float>(SettingId.Resolution)
                .Subscribe(OnResolutionSetting).AddTo(_disposable);
            _settingsService.ObserveEnum<FrameRateMode>(SettingId.TargetFrameRate)
                .Subscribe(OnFrameRateSetting).AddTo(_disposable);
        }

        private void OnFullScreenModeSetting(FullScreenMode mode)
        {
            _applicationService.SetFullscreenMode(mode);
        }

        private void OnResolutionSetting(float resolutionIdx)
        {
            var resolutions = _applicationService.AvailableResolutions;
            if ((int)resolutionIdx == -1)
            {
                _applicationService.TrySetResolution(resolutions[^1]);
                return;
            }
            var resolution = resolutions[Mathf.Clamp((int)resolutionIdx, 0, resolutions.Count - 1)];
            _applicationService.TrySetResolution(resolution);
        }
        
        private void OnFrameRateSetting(FrameRateMode mode)
        {
            switch (mode)
            {
                case FrameRateMode.VSync: _applicationService.EnableVSync(); break;
                case FrameRateMode.Fps30: _applicationService.SetTargetFrameRate(30); break;
                case FrameRateMode.Fps60: _applicationService.SetTargetFrameRate(60); break;
                case FrameRateMode.Fps120: _applicationService.SetTargetFrameRate(120); break;
                case FrameRateMode.Fps144: _applicationService.SetTargetFrameRate(144); break;
            }
        }
        
        protected override void OnUpdate() { }

        protected override void OnDestroy()
        {
            _disposable?.Dispose();
        }
    }
}