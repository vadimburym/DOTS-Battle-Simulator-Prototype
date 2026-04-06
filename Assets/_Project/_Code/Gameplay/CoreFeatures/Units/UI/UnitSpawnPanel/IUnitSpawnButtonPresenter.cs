using UnityEngine;

namespace _Project._Code.Gameplay.CoreFeatures.Units.UI
{
    public interface IUnitSpawnButtonPresenter
    {
        Sprite Icon { get; }
        void OnSpawnDataClicked(int count);
    }
}