using R3;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Service
{
    public interface IUnitCounterService
    {
        ReadOnlyReactiveProperty<int> UnitsCount { get; }
        void Increase(int count);
        void Decrease(int count);
    }
}