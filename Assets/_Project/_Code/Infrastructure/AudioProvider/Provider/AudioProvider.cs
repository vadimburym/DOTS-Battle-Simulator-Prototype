using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Audio;

namespace _Project._Code.Infrastructure.Audio
{
    public sealed class AudioProvider : SerializedMonoBehaviour, IAudioProvider
    {
        private const float CHANNEL_VOLUME_MAXIMUM = 0.0f;
        private const float CHANNEL_VOLUME_MINIMUM = -80.0f;
        
        [SerializeField] private AudioMixer _audioMixer;
        [OdinSerialize] private Dictionary<AudioOutput, string> _audioGroupsName;
        [OdinSerialize] private Dictionary<AudioOutput, AudioSourceAdapter> _audioSources;
        [SerializeField] private AudioSourcePool _audioSourcePool;

        private readonly Dictionary<AudioOutput, AudioSourceSnapshot> _defaultSnapshots = new();
        private readonly Dictionary<AudioOutput, AudioMixerGroup> _audioMixerGroups = new();
        
        private void Awake()
        {
            foreach (var source in _audioSources)
                _defaultSnapshots[source.Key] = source.Value.GetSnapshot();
            var outputs = EnumUtils<AudioOutput>.Values;
            for (int i = 0; i < outputs.Length; i++)
            {
                var outputName = EnumUtils<AudioOutput>.ToString(outputs[i]);
                var mixerGroups = _audioMixer.FindMatchingGroups(outputName);
                if (mixerGroups == null)
                    Debug.LogError($"There is no output {outputName}");
                else
                    _audioMixerGroups.Add(outputs[i], mixerGroups[0]);
            }
        }
        
        public void SetAudioOutputVolume(AudioOutput output, float value)
        {
            if (_audioGroupsName.TryGetValue(output, out var groupName))
                _audioMixer.SetFloat(groupName, Mathf.Clamp(value, CHANNEL_VOLUME_MINIMUM, CHANNEL_VOLUME_MAXIMUM));
        }

        public void Play(
            AudioOutput output,
            AudioClip clip,
            float volumeScale = 1.0f,
            AudioSourceSnapshot snapshot = null)
        {
            var source = _audioSources[output];
            source.SetAudioSourceSnapshot(snapshot ?? _defaultSnapshots[output]);
            source.Play(clip, volumeScale);
        }

        public void PlayOneShot(
            AudioOutput output,
            AudioClip clip,
            float volumeScale = 1.0f,
            float pitch = 1.0f,
            AudioSourceSnapshot snapshot = null)
        {
            var source = _audioSources[output];
            source.SetAudioSourceSnapshot(snapshot ?? _defaultSnapshots[output]);
            source.PlayOneShot(clip, volumeScale, pitch);
        }

        public void Play(
            AudioOutput output,
            AudioClip clip,
            Vector3 position,
            float volumeScale = 1.0f,
            AudioSourceSnapshot snapshot = null)
        {
            var source = _audioSourcePool.GetAudioSource();
            source.SetAudioSourceOutput(_audioMixerGroups[output]);
            source.SetAudioSourceSnapshot(snapshot ?? _defaultSnapshots[output]);
            source.SetPosition(position);
            source.Play(clip, volumeScale);
        }
        
        public void Play(
            AudioOutput output,
            AudioClip clip,
            Transform parent,
            float volumeScale = 1.0f,
            AudioSourceSnapshot snapshot = null)
        {
            var source = _audioSourcePool.GetAudioSource();
            source.SetAudioSourceOutput(_audioMixerGroups[output]);
            source.SetAudioSourceSnapshot(snapshot ?? _defaultSnapshots[output]);
            source.SetParent(parent, false);
            source.Play(clip, volumeScale);
        }
    }
}