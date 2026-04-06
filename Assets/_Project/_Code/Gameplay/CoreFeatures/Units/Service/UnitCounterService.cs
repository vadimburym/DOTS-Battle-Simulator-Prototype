using R3;

namespace _Project._Code.Gameplay.CoreFeatures.Units.Service
{
    public sealed class UnitCounterService : IUnitCounterService
    {
        public ReadOnlyReactiveProperty<int> UnitsCount => _unitsCount;
        private readonly ReactiveProperty<int> _unitsCount = new();
        
        public void Increase(int count) => _unitsCount.Value += count;
        public void Decrease(int count) => _unitsCount.Value -= count;
    }
}