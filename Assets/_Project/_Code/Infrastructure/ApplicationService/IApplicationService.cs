using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project._Code.Infrastructure.ApplicationService
{
    public interface IApplicationService
    {
        IReadOnlyList<ResolutionOption> AvailableResolutions { get; }
        IReadOnlyList<string> QualityLevels { get; }
        void SetFullscreenMode(FullScreenMode mode);
        bool TrySetResolution(ResolutionOption resolution, FullScreenMode? overrideMode = null);
        void SetVSyncCount(int count);
        void EnableVSync(int count = 1);
        void DisableVSync();
        void SetTargetFrameRate(int fps, bool disableVSync = true);
        void ResetTargetFrameRate();
        bool TrySetQualityLevel(int index, bool applyExpensiveChanges = true);
        void Quit();
    }
}