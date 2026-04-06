using Unity.Entities;
using UnityEngine;
using VContainer;

namespace _Project._Code.Tools.FrameDebug
{
    [DisableAutoCreation]
    public partial class FrameDebugSystem : SystemBase
    {
        private float _frameMs;
        private float _fps;
        
        private IFrameDebugProvider _frameDebugProvider;
        
        [Inject]
        public void Construct(IFrameDebugProvider frameDebugProvider)
        {
            _frameDebugProvider = frameDebugProvider;
        }
        
        protected override void OnUpdate()
        {
            if (!_frameDebugProvider.Enabled)
                return;

            float dt = UnityEngine.Time.unscaledDeltaTime;
            if (dt <= 0f)
                return;

            float currentMs = dt * 1000f;
            float currentFps = 1f / dt;

            if (_frameDebugProvider.Smooth)
            {
                _frameMs = Mathf.Lerp(_frameMs, currentMs, _frameDebugProvider.SmoothFactor);
                _fps = Mathf.Lerp(_fps, currentFps, _frameDebugProvider.SmoothFactor);
            }
            else
            {
                _frameMs = currentMs;
                _fps = currentFps;
            }
            string text = $"Frame: {_frameMs:F2} ms\nFPS: {_fps:F1}";
            _frameDebugProvider.SetFrameDebugText(text);
        }
    }
}