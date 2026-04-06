using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project._Code.Global.SettingsService.UI.Settings
{
    public sealed class NumericSettingView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _settingName;
        [SerializeField] private Slider _slider;

        private INumericSettingPresenter _presenter;
        private readonly CompositeDisposable _compositeDisposable = new();

        public void Initialize(INumericSettingPresenter presenter)
        {
            _presenter = presenter;
            _slider.minValue = _presenter.MinValue;
            _slider.maxValue = _presenter.MaxValue;
            _slider.wholeNumbers = _presenter.WholeNumbers;
        }

        private void OnEnable()
        {
            if (_presenter == null)
                return;
            _slider.onValueChanged.AddListener(_presenter.OnValueChangedFromSlider);
            _presenter.SettingName.Subscribe(value => _settingName.text = value).AddTo(_compositeDisposable);
            _presenter.FloatValue.Subscribe(value => _slider.SetValueWithoutNotify(value));
        }

        private void OnDisable()
        {
            if (_presenter == null)
                return;
            _compositeDisposable.Clear();
            _slider.onValueChanged.RemoveListener(_presenter.OnValueChangedFromSlider);
        }
    }
}