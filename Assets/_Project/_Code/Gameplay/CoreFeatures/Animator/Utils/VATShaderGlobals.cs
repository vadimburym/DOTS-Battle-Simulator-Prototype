using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VATDots
{
    public static class VATShaderGlobals
    {
        private static readonly int AnimStateBufferId = Shader.PropertyToID("_VATAnimStateBuffer");
        private static GraphicsBuffer _fallbackBuffer;
        private static bool _initialized;
        private const int StrideBytes = 32;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInit()
        {
            EnsureInitialized();
            RebindFallback();
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void EditorInit()
        {
            EnsureInitialized();
            RebindFallback();

            AssemblyReloadEvents.beforeAssemblyReload -= DisposeFallback;
            AssemblyReloadEvents.beforeAssemblyReload += DisposeFallback;

            EditorApplication.quitting -= DisposeFallback;
            EditorApplication.quitting += DisposeFallback;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                case PlayModeStateChange.EnteredPlayMode:
                case PlayModeStateChange.ExitingPlayMode:
                case PlayModeStateChange.EnteredEditMode:
                    EnsureInitialized();
                    RebindFallback();
                    EditorApplication.delayCall -= DelayedRebind;
                    EditorApplication.delayCall += DelayedRebind;
                    break;
            }
        }

        private static void DelayedRebind()
        {
            EnsureInitialized();
            RebindFallback();
        }
#endif

        public static void EnsureInitialized()
        {
            if (_initialized && _fallbackBuffer != null)
            {
                Shader.SetGlobalBuffer(AnimStateBufferId, _fallbackBuffer);
                return;
            }

            if (_fallbackBuffer != null)
            {
                _fallbackBuffer.Dispose();
                _fallbackBuffer = null;
            }

            _fallbackBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, StrideBytes);
            _fallbackBuffer.SetData(new float[8]);
            _initialized = true;
            Shader.SetGlobalBuffer(AnimStateBufferId, _fallbackBuffer);
        }

        public static void Bind(GraphicsBuffer buffer)
        {
            EnsureInitialized();
            Shader.SetGlobalBuffer(AnimStateBufferId, buffer != null ? buffer : _fallbackBuffer);
        }

        public static void RebindFallback()
        {
            EnsureInitialized();
            Shader.SetGlobalBuffer(AnimStateBufferId, _fallbackBuffer);
        }

        public static void DisposeFallback()
        {
            if (_fallbackBuffer != null)
            {
                _fallbackBuffer.Dispose();
                _fallbackBuffer = null;
            }

            _initialized = false;
        }
    }
}
