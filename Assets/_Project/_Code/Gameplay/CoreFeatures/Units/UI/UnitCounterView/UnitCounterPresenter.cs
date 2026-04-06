using _Project._Code.Gameplay.CoreFeatures.Units.Service;
using R3;

namespace _Project._Code.Gameplay.CoreFeatures.Units.UI
{
    public sealed class UnitCounterPresenter : IUnitCounterPresenter
    {
        public ReadOnlyReactiveProperty<string> CounterText => _counterText;
        private readonly ReactiveProperty<string> _counterText = new("");
        
        private readonly CompositeDisposable _disposables;
        
        public UnitCounterPresenter(IUnitCounterService model)
        {
            _disposables = new ();
            model.UnitsCount.Subscribe(value => _counterText.Value = value.ToString()).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}