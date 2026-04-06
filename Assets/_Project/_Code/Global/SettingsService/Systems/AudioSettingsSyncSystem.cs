using _Project._Code.Core.Keys;
using _Project._Code.Infrastructure.Audio;
using R3;
using Unity.Entities;
using VContainer;

namespace _Project._Code.Global.Settings
{
    [DisableAutoCreation]
    public partial class AudioSettingsSyncSystem : SystemBase
    {
        private IAudioProvider _audioProvider;
        private ISettingsService _settingsService;
        private CompositeDisposable _disposable;
        
        [Inject]
        private void Construct(
            IAudioProvider audioProvider,
            ISettingsService settingsService)
        {
            _disposable = new CompositeDisposable();
            _audioProvider = audioProvider;
            _settingsService = settingsService;
            _settingsService.Observe<float>(SettingId.MasterVolume)
                .Subscribe(value => OnVolumeSetting(AudioOutput.Master, value)).AddTo(_disposable);
            _settingsService.Observe<float>(SettingId.MusicVolume)
                .Subscribe(value => OnVolumeSetting(AudioOutput.Music, value)).AddTo(_disposable);
            _settingsService.Observe<float>(SettingId.UIVolume)
                .Subscribe(value => OnVolumeSetting(AudioOutput.UI, value)).AddTo(_disposable);
            _settingsService.Observe<float>(SettingId.EnvironmentVolume)
                .Subscribe(value => OnVolumeSetting(AudioOutput.Environment, value)).AddTo(_disposable);
            _settingsService.Observe<float>(SettingId.SfxVolume)
                .Subscribe(value => OnVolumeSetting(AudioOutput.SFX, value)).AddTo(_disposable);
        }

        private void OnVolumeSetting(AudioOutput output, float volume)
        {
            _audioProvider.SetAudioOutputVolume(output, volume);
        }
        
        protected override void OnUpdate() { }

        protected override void OnDestroy()
        {
            _disposable?.Dispose();
        }
    }
}