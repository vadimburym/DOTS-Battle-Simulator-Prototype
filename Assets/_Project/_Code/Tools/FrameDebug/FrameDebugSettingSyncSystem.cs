using _Project._Code.Core.Keys;
using _Project._Code.Global.Settings;
using Unity.Entities;
using VContainer;
using R3;

namespace _Project._Code.Tools.FrameDebug
{
    [DisableAutoCreation]
    public partial class FrameDebugSettingSyncSystem : SystemBase
    {
        private IFrameDebugProvider _frameDebugProvider;
        private ISettingsService _settingsService;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        
        [Inject]
        private void Construct(
            IFrameDebugProvider frameDebugProvider,
            ISettingsService settingsService)
        {
            _frameDebugProvider = frameDebugProvider;
            _settingsService = settingsService;
            _settingsService.Observe<bool>(SettingId.FrameDebug)
                .Subscribe(OnFrameDebug).AddTo(_disposables);
        }

        private void OnFrameDebug(bool enabled)
        {
            _frameDebugProvider.SetEnabled(enabled);
        }
        
        protected override void OnUpdate() { }

        protected override void OnDestroy()
        {
            _disposables.Dispose();   
        }
    }
}