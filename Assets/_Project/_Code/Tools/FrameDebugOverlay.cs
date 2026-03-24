using UnityEngine;

public sealed class FrameDebugOverlay : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private Vector2 screenPosition = new Vector2(20f, 20f);
    [SerializeField] private int fontSize = 24;
    [SerializeField] private Color textColor = Color.white;
    [Header("Smoothing")]
    [SerializeField] private bool smooth = true;
    [SerializeField, Range(0.01f, 1f)] private float smoothFactor = 0.1f;
    
    private float _frameMs;
    private float _fps;
    private GUIStyle _style;

    private void Awake()
    {
        _style = new GUIStyle
        {
            fontSize = fontSize,
            normal = { textColor = textColor }
        };
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        if (dt <= 0f)
            return;

        float currentMs = dt * 1000f;
        float currentFps = 1f / dt;

        if (smooth)
        {
            _frameMs = Mathf.Lerp(_frameMs, currentMs, smoothFactor);
            _fps = Mathf.Lerp(_fps, currentFps, smoothFactor);
        }
        else
        {
            _frameMs = currentMs;
            _fps = currentFps;
        }
    }

    private void OnGUI()
    {
        if (_style == null)
        {
            _style = new GUIStyle
            {
                fontSize = fontSize,
                normal = { textColor = textColor }
            };
        }

        string text = $"Frame: {_frameMs:F2} ms\nFPS: {_fps:F1}";
        GUI.Label(new Rect(screenPosition.x, screenPosition.y, 250f, 80f), text, _style);
    }
}