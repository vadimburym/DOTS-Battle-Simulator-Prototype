using System;
using UnityEngine;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Behaviours
{
    public interface ISelectionAreaProvider
    {
        event Action<SelectionResult> OnSelectionResult;
        bool InBounds(Vector2 screenPoint);
    }
}