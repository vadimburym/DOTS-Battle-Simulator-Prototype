using UnityEngine;

namespace _Project._Code.Infrastructure.Audio
{
    public interface IAudioProvider
    {
        void SetAudioOutputVolume(AudioOutput output, float value);
        void Play(AudioOutput output, AudioClip clip, float volumeScale = 1.0f, AudioSourceSnapshot snapshot = null);
        void PlayOneShot(AudioOutput output, AudioClip clip, float volumeScale = 1.0f, float pitch = 1.0f,
            AudioSourceSnapshot snapshot = null);
        void Play(AudioOutput output, AudioClip clip, Vector3 position, float volumeScale = 1.0f,
            AudioSourceSnapshot snapshot = null);
        void Play(AudioOutput output, AudioClip clip, Transform parent, float volumeScale = 1.0f,
            AudioSourceSnapshot snapshot = null);
    }
}