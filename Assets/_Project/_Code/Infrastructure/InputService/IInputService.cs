using UnityEngine;

namespace _Project._Code.Infrastructure
{
    public interface IInputService
    {
        bool IsMainActionDown { get; }
        bool IsMainActionUp { get; }
        bool IsSecondActionDown { get; }
        bool TryGetMouseToWorldPosition(out Vector3 worldPosition);
    }
}