using System;
using UnityEngine;

namespace _Project._Code.Infrastructure.ApplicationService
{
    [Serializable]
    public readonly struct ApplicationSnapshot
    {
        public ResolutionOption Resolution { get; }
        public FullScreenMode FullScreenMode { get; }
        public bool IsFullscreen { get; }
        public int VSyncCount { get; }
        public int TargetFrameRate { get; }
        public int QualityLevelIndex { get; }

        public ApplicationSnapshot(
            ResolutionOption resolution,
            FullScreenMode fullScreenMode,
            bool isFullscreen,
            int vSyncCount,
            int targetFrameRate,
            int qualityLevelIndex)
        {
            Resolution = resolution;
            FullScreenMode = fullScreenMode;
            IsFullscreen = isFullscreen;
            VSyncCount = vSyncCount;
            TargetFrameRate = targetFrameRate;
            QualityLevelIndex = qualityLevelIndex;
        }
    }
}