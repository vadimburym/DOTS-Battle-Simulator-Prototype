using System;
using UnityEngine;

namespace _Project._Code.Infrastructure
{
    public interface IInputService
    {
        event Action OnEscape;
        bool IsMainActionDown { get; }
        bool IsMainActionUp { get; }
        bool IsSecondActionDown { get; }
        bool IsEscape { get; }
        bool TryGetMouseToWorldPosition(out Vector3 worldPosition);
        Vector2 MousePosition { get; }
        float Scroll { get; }
        void InputEscape();
    }
}