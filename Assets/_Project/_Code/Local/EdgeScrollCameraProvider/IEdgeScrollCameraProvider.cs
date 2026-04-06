using UnityEngine;

namespace _Project._Code.Locale.EdgeScrollCamera
{
    public interface IEdgeScrollCameraProvider
    {
        float MoveSpeed { get; }
        float EdgeSize { get; }
        Vector2 XLimits { get; }
        Vector2 ZLimits { get; }
        float ZoomSpeed { get; }
        float MinY { get; }
        float MaxY { get; }
    }
}