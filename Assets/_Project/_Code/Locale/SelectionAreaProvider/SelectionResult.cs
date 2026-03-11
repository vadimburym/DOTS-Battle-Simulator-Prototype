using System;
using UnityEngine;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Behaviours
{
    [Serializable]
    public struct SelectionResult
    {
        public Vector2 LocalMin;
        public Vector2 LocalMax;
        public Vector2 ScreenMin;
        public Vector2 ScreenMax;
        public Vector2 ScreenTopLeft => new(ScreenMin.x, ScreenMax.y);
        public Vector2 ScreenBottomRight => new(ScreenMax.x, ScreenMin.y);
        public Rect LocalRect => Rect.MinMaxRect(LocalMin.x, LocalMin.y, LocalMax.x, LocalMax.y);
        public Rect ScreenRect => Rect.MinMaxRect(ScreenMin.x, ScreenMin.y, ScreenMax.x, ScreenMax.y);
    }
}