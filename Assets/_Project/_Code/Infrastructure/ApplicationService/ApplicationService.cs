using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Project._Code.Infrastructure.ApplicationService
{
    public sealed class ApplicationService : IApplicationService
    {
        public ApplicationSnapshot Current => CaptureCurrent();

        public IReadOnlyList<string> QualityLevels => QualitySettings.names;
        public IReadOnlyList<ResolutionOption> AvailableResolutions => _resolutionOptions;
        
        private readonly List<ResolutionOption> _resolutionOptions = new();

        public ApplicationService()
        {
            var source = Screen.resolutions;
            for (int i = 0; i < source.Length; i++)
                _resolutionOptions.Add(ResolutionOption.FromUnity(source[i]));
        }

        public ResolutionOption GetResolutionOption(int index)
        {
            return _resolutionOptions[Mathf.Clamp(index, 0, _resolutionOptions.Count - 1)];
        }
        
        public void SetFullscreenMode(FullScreenMode mode)
        {
            var currentResolution = CaptureCurrent().Resolution;

            if (mode == FullScreenMode.ExclusiveFullScreen)
            {
                if (!IsSupportedExclusiveResolution(currentResolution))
                    return;
            }

            Screen.SetResolution(
                currentResolution.Width,
                currentResolution.Height,
                mode,
                currentResolution.ToRefreshRate());
        }

        public bool TrySetResolution(ResolutionOption resolution, FullScreenMode? overrideMode = null)
        {
            if (resolution.Width <= 0 || resolution.Height <= 0)
                return false;

            var mode = overrideMode ?? Screen.fullScreenMode;

            if (mode == FullScreenMode.ExclusiveFullScreen && !IsSupportedExclusiveResolution(resolution))
                return false;

            if (mode == FullScreenMode.ExclusiveFullScreen)
            {
                Screen.SetResolution(
                    resolution.Width,
                    resolution.Height,
                    mode,
                    resolution.ToRefreshRate());
            }
            else
            {
                Screen.SetResolution(
                    resolution.Width,
                    resolution.Height,
                    mode);
            }

            return true;
        }

        public void SetVSyncCount(int count)
        {
            QualitySettings.vSyncCount = Mathf.Clamp(count, 0, 4);
            Application.targetFrameRate = -1;
        }

        public void EnableVSync(int count = 1)
        {
            SetVSyncCount(Mathf.Clamp(count, 1, 4));
        }

        public void DisableVSync()
        {
            SetVSyncCount(0);
        }

        public bool IsFrameRateEditable()
        {
            return QualitySettings.vSyncCount == 0;
        }

        public void SetTargetFrameRate(int fps, bool disableVSync = true)
        {
            if (fps != -1 && fps <= 0)
                throw new ArgumentOutOfRangeException(nameof(fps), "FPS must be -1 or > 0.");

            if (disableVSync && QualitySettings.vSyncCount > 0)
                DisableVSync();

            Application.targetFrameRate = fps;
        }

        public void ResetTargetFrameRate()
        {
            Application.targetFrameRate = -1;
        }

        public bool TrySetQualityLevel(int index, bool applyExpensiveChanges = true)
        {
            if (index < 0 || index >= QualitySettings.names.Length)
                return false;

            QualitySettings.SetQualityLevel(index, applyExpensiveChanges);
            return true;
        }
        
        public void Quit()
        {
    #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
        }
        
        private static ApplicationSnapshot CaptureCurrent()
        {
            return new ApplicationSnapshot(
                resolution: ResolutionOption.FromCurrentResolution(),
                fullScreenMode: Screen.fullScreenMode,
                isFullscreen: Screen.fullScreen,
                vSyncCount: QualitySettings.vSyncCount,
                targetFrameRate: Application.targetFrameRate,
                qualityLevelIndex: QualitySettings.GetQualityLevel());
        }

        private static bool IsSupportedExclusiveResolution(ResolutionOption resolution)
        {
            var supported = Screen.resolutions;
            for (int i = 0; i < supported.Length; i++)
            {
                var x = ResolutionOption.FromUnity(supported[i]);
                if (x.Equals(resolution))
                    return true;
            }
            return false;
        }
    }
}