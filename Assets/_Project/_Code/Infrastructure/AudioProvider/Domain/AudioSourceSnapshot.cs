using System;
using UnityEngine;

namespace _Project._Code.Infrastructure.Audio
{
    [Serializable]
    public sealed class AudioSourceSnapshot
    {
        public bool IsWaitingPlayingEnd = false;
        public bool IsLoop = false;
        public bool BypassEffects = false;
        public bool BypassListenerEffects = false;
        public bool BypassReverbZones = false;
        [Range(0f, 256f)] public float Priority = 128;
        [Range(-1f, 1f)] public float StereoPan = 0;
        [Range(0f, 1f)] public float SpatialBlend = 0;
        [Range(0f, 1.1f)] public float ReverbZoneMix = 1;
    }
}