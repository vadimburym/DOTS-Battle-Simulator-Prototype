public interface IFrameDebugProvider
{
    bool Smooth { get; }
    float SmoothFactor { get; }
    bool Enabled { get; }
    void SetFrameDebugText(string text);
    void SetEnabled(bool isEnabled);
}