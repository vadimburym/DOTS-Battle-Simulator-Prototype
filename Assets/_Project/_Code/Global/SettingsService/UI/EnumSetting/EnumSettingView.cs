using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project._Code.Global.SettingsService.UI.EnumSetting
{
    public sealed class EnumSettingView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _settingName;
        [SerializeField] private TMP_Text _currentEnumName;
        [SerializeField] private Button _leftButton;
        [SerializeField] private Button _rightButton;

        private IEnumSettingPresenter _presenter;
        private readonly CompositeDisposable _compositeDisposable = new();

        public void Initialize(IEnumSettingPresenter presenter)
        {
            _presenter = presenter;
        }

        private void OnEnable()
        {
            if (_presenter == null)
                return;
            _presenter.SettingName.Subscribe(value => _settingName.text = value).AddTo(_compositeDisposable);
            _presenter.CurrentEnumName.Subscribe(value => _currentEnumName.text = value).AddTo(_compositeDisposable);
            _leftButton.onClick.AddListener(_presenter.OnLeftButtonClicked);
            _rightButton.onClick.AddListener(_presenter.OnRightButtonClicked);
        }

        private void OnDisable()
        {
            if (_presenter == null)
                return;
            _compositeDisposable.Clear();
            _leftButton.onClick.RemoveListener(_presenter.OnLeftButtonClicked);
            _rightButton.onClick.RemoveListener(_presenter.OnRightButtonClicked);
        }
    }
}