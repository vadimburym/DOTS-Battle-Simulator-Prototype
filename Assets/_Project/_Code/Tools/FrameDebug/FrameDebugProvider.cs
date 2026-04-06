using TMPro;
using UnityEngine;

public sealed class FrameDebugProvider : MonoBehaviour, IFrameDebugProvider
{
    public bool Smooth => _smooth;
    public float SmoothFactor => _smoothFactor;
    public bool Enabled => _enabled;
    
    [Header("Display")]
    [SerializeField] private TMP_Text _debugText;
    [Header("Smoothing")]
    [SerializeField] private bool _smooth = true;
    [SerializeField, Range(0.01f, 1f)] private float _smoothFactor = 0.1f;

    private bool _enabled;
    
    public void SetFrameDebugText(string text) => _debugText.text = text;
    public void SetEnabled(bool isEnabled)
    { 
        _enabled = isEnabled;
        gameObject.SetActive(_enabled);
    } 
}