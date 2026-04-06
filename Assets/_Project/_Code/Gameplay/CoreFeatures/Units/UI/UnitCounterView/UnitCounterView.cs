using _Project._Code.Locale;
using R3;
using TMPro;
using UnityEngine;

namespace _Project._Code.Gameplay.CoreFeatures.Units.UI
{
    public sealed class UnitCounterView : MonoWidget<IUnitCounterPresenter>
    {
        [SerializeField] private TMP_Text _unitCountText;

        private IUnitCounterPresenter _presenter;
        private CompositeDisposable _disposables;
        
        public override void Initialize(IUnitCounterPresenter presenter)
        {
            _presenter = presenter;
            _disposables = new ();
        }

        private void OnEnable()
        {
            if (_presenter == null)
                return;
            _presenter.CounterText.Subscribe(value => _unitCountText.text = value).AddTo(_disposables);
        }

        private void OnDisable()
        {
            if (_presenter == null)
                return;
            _disposables.Clear();
        }
    }
}