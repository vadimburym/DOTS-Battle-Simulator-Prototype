using System;

namespace _Project._Code.Gameplay.CoreFeatures.Entities.Behaviours
{
    public interface ISelectionAreaProvider
    {
        event Action<SelectionResult> OnSelectionResult;
    }
}