using UnityEngine;
using UnityEngine.Audio;

namespace _Project._Code.Infrastructure.Audio
{
    public sealed class AudioSourceAdapter : MonoBehaviour
    {
        [SerializeField] private AudioSource _source;
        private bool _isWaitingPlayingEnd;
        private bool _isParentDefault;
        public bool IsPlaying => _source.isPlaying;

        public AudioSourceSnapshot GetSnapshot()
        {
            return new AudioSourceSnapshot
            {
                IsWaitingPlayingEnd = _isWaitingPlayingEnd,
                IsLoop = _source.loop,
                BypassEffects = _source.bypassEffects,
                BypassListenerEffects = _source.bypassListenerEffects,
                BypassReverbZones = _source.bypassReverbZones,
                Priority = _source.priority,
                StereoPan = _source.panStereo,
                SpatialBlend = _source.spatialBlend,
                ReverbZoneMix = _source.reverbZoneMix
            };
        }
        
        public void SetAudioSourceSnapshot(AudioSourceSnapshot snapshot)
        {
            _isWaitingPlayingEnd = snapshot.IsWaitingPlayingEnd;
            _source.loop = snapshot.IsLoop;
            _source.bypassEffects = snapshot.BypassEffects;
            _source.bypassListenerEffects = snapshot.BypassListenerEffects;
            _source.bypassReverbZones = snapshot.BypassReverbZones;
            _source.priority = Mathf.RoundToInt(snapshot.Priority);
            _source.panStereo = snapshot.StereoPan;
            _source.spatialBlend = snapshot.SpatialBlend;
            _source.reverbZoneMix = snapshot.ReverbZoneMix;
        }

        public void SetAudioSourceOutput(AudioMixerGroup output)
        {
            gameObject.name = $"AudioSource <{output.name}>";
            _source.outputAudioMixerGroup = output;
        }

        public void PlayOneShot(AudioClip clip, float volumeScale, float pitch)
        {
            _source.pitch = pitch;
            _source.PlayOneShot(clip, volumeScale);
        }

        public void Play(AudioClip clip, float volumeScale)
        {
            if (_isWaitingPlayingEnd && _source.isPlaying)
                return;
            _source.clip = clip;
            _source.volume = volumeScale;
            _source.Play();
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetParent(Transform parent, bool isDefault)
        {
            if (_isParentDefault && isDefault)
                return;
            _isParentDefault = isDefault;
            transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
        }
    }
}